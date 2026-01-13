using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

namespace D_OS_Save_Editor
{
    /// <summary>
    /// Interaction logic for Inventory.xaml
    /// </summary>
    public partial class Page1
    {
        private Player _player;
        private List<ItemTemplate> _NewItems;
        private List<ItemTemplate> _AddedItems;

        private Brush DefaultTextBoxBorderBrush { get; }
        private Brush[] _itemRarityColor =
            {Brushes.Black, Brushes.ForestGreen, Brushes.DodgerBlue, Brushes.BlueViolet, Brushes.DeepPink, Brushes.Gold, Brushes.DimGray};

        public Player Player
        {
            get => _player;

            set
            {
                _player = value;
                UpdateForm();
            }
        }

        public List<ItemTemplate> NewItems
        {
            get => _NewItems;

            set
            {
                _NewItems = value;
                UpdateForm();
            }
        }

        public List<ItemTemplate> AddedItems
        {
            get => _AddedItems;

            set
            {
                _AddedItems = value;
                UpdateForm();
            }
        }

        public Page1()
        {
            InitializeComponent();
            DefaultTextBoxBorderBrush = AmountTextBox.BorderBrush;

            RarityComboBox.ItemsSource = Enum.GetValues(typeof(Item.ItemRarityType)).Cast<Item.ItemRarityType>();
            _AddedItems = new List<ItemTemplate>();
        }

        public void UpdateForm()
        {
            ItemsListBox.Items.Clear();
            foreach (var i in _NewItems)
                ItemsListBox.Items.Add(new ListBoxItem
                {
                    Content = i.Name,
                    Tag = i.ItemSort,
                    Foreground = _itemRarityColor[0]
                });

            SelectedItemsListbox.Items.Clear();

            //_AddedItems.Add(new ItemTemplate("name", "Considered a beverage by the hale and suicidal Dwarves of the frozen north.", "23276KEY", "999"));

            if (_AddedItems != null)
            {
                foreach (var i in _AddedItems)
                    SelectedItemsListbox.Items.Add(i);
            }

            // check filter
            foreach (var i in ShowWrapPanel.Children)
            {
                if (i is CheckBox)
                    CheckboxEventSetter_OnClick(i, new RoutedEventArgs());
            }

            // clear all text boxes
            foreach (var i in ValueWrapPanel.Children)
            {
                if (i is TextBox t)
                    t.Text = "";
            }
        }

        private void DecrementButton_Click(object sender, RoutedEventArgs e) {

            var button = sender as FrameworkElement;
            if (button == null)
                return;

            var item = button.DataContext as ItemTemplate;
            if (item == null) return;
            Console.WriteLine("");
            Console.WriteLine("");

        }

        private void TextBoxEventSetter_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (!(sender is TextBox s)) return;
            if (s.Uid == "SearchText") return;

            var text = s.Text;
            var valid = int.TryParse(text, out int _);
            s.BorderBrush = !valid ? Brushes.Red : DefaultTextBoxBorderBrush;
        }

        private void TextBoxEventSetter_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(sender is TextBox s)) return;
            if (s.Uid == "SearchText") return;

            var text = s.Text.Insert(s.SelectionStart, e.Text);
            e.Handled = !int.TryParse(text, out int _);
        }

        private void ItemsListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // clear list boxes
            var lb = sender as ListBox;
            if (lb.SelectedIndex < 0)
                return;
            var item = _NewItems[lb.SelectedIndex];

#if DEBUG
            Console.WriteLine("Item name: "+item.Name.ToString());
            Console.WriteLine("Item Mapkey: "+item.TemplateKey.ToString());
            if (item.Description != null) Console.WriteLine("Item Description: " + item.Description.ToString());
            else Console.WriteLine("Description Null");
            if (item.MaxStack != null) Console.WriteLine("Max Stack: " + item.MaxStack.ToString());
            else Console.WriteLine("MaxStack Null");
#endif
            _AddedItems.Add(item.DeepClone());
            UpdateForm();

            //var allowedChanges = item.GetAllowedChangeType();
            #region enable disable controls

            //RarityComboBox.IsEnabled = allowedChanges.Contains(nameof(item.ItemRarity));
            AmountTextBox.IsEnabled = true;

            #endregion

#if DEBUG && LOG_ITEMXML
            Console.WriteLine(item.Xml);
#endif
            // textbox contents
            //AmountTextBox.Text = item.Amount;

            // combobox
            //RarityComboBox.SelectedIndex = (int)item.ItemRarity;

        }

        private void CheckboxEventSetter_OnClick(object sender, RoutedEventArgs e)
        {
            var ckb = sender as CheckBox;
            if (!(ckb?.Tag is ItemSortType))
                return;

            if ((ItemSortType)ckb.Tag == ItemSortType.Other)
            {
                foreach (ListBoxItem i in ItemsListBox.Items)
                {
                    if ((ItemSortType)i.Tag == ItemSortType.Item || (ItemSortType)i.Tag == ItemSortType.Unique ||
                        (ItemSortType)i.Tag == ItemSortType.Other)
                        i.Visibility = ckb.IsChecked == true && !IsFilteredOutByText(i.Content as string) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            else
            {
                foreach (ListBoxItem i in ItemsListBox.Items)
                {
                    if ((ItemSortType)i.Tag == (ItemSortType)ckb.Tag)
                        i.Visibility = ckb.IsChecked == true && !IsFilteredOutByText(i.Content as string) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private bool IsFilteredOutByText(string itemName)
        {
            var isFilteredOut = false;
            itemName = itemName.ToLower();
            var searchTerms = SearchTextBox.Text.ToLower().Split(' ');
            foreach (var s in searchTerms)
            {
                if (itemName.Contains(s)) continue;

                isFilteredOut = true;
                break;
            }

            return isFilteredOut;
        }

        private void CheckAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var i in ShowWrapPanel.Children)
            {
                if (!(i is CheckBox box)) continue;
                box.IsChecked = true;
                CheckboxEventSetter_OnClick(i, new RoutedEventArgs());
            }
        }

        private void UncheckAllButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var i in ShowWrapPanel.Children)
            {
                if (!(i is CheckBox box)) continue;
                box.IsChecked = false;
                CheckboxEventSetter_OnClick(i, new RoutedEventArgs());
            }
        }

        private void ApplyChangesButton_OnClick(object sender, RoutedEventArgs e)
        {
            
            try
            {
                if (_AddedItems.Count < 1) return;
                foreach (var i in _AddedItems) 
                {
                    string slot = getEmptySlot(_player);
                    ItemTemplate temp = new ItemTemplate(i.Name,i.Description,i.TemplateKey,i.MaxStack,i.Stats,4);
                    ItemChange change = new ItemChange(temp,ChangeType.Add);
                    _player.ItemChanges.Add(slot,change);
                
                }

                var tooltip = new ToolTip { Content = "Changes have been applied!" };
                ((Button)sender).ToolTip = tooltip;
                tooltip.Opened += async delegate (object o, RoutedEventArgs args)
                {
                    var s = o as ToolTip;
                    await Task.Delay(1000);
                    s.IsOpen = false;
                    await Task.Delay(1000);
                    ((Button)sender).ClearValue(ToolTipProperty);
                };
                tooltip.IsOpen = true;
            }
            catch (XmlValidationException ex)
            {
                MessageBox.Show($"Invalid value entered: {ex.Name}: {ex.Value}. No change has been applied.\n\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Internal error. No change has been applied.\n\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (ListBoxItem i in ItemsListBox.Items)
            {
                var listBoxText = ((string)i.Content).ToLower();
                var searchTerms = SearchTextBox.Text.ToLower().Split(' ');
                var visibility = Visibility.Visible;
                foreach (var s in searchTerms)
                {
                    if (listBoxText.Contains(s)) continue;

                    visibility = Visibility.Collapsed;
                    break;
                }
                i.Visibility = visibility;
            }
        }

        public static string getEmptySlot(Player player)
        {

            int emptySlot = 20;
            while (emptySlot < 16300)
            {
                if (!player.SlotsOccupation[emptySlot])
                {
                    player.SlotsOccupation[emptySlot] = true;
                    return emptySlot.ToString();
                }
                else emptySlot++;
            }
            throw new Exception("Can't find empty slot");
        }
    }
}
