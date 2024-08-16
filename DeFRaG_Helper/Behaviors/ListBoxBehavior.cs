using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace DeFRaG_Helper.Behaviors
{
    public static class ListBoxBehavior
    {
        public static readonly DependencyProperty BindableSelectedItemsProperty =
            DependencyProperty.RegisterAttached("BindableSelectedItems", typeof(IList), typeof(ListBoxBehavior), new PropertyMetadata(null, OnBindableSelectedItemsChanged));

        public static IList GetBindableSelectedItems(DependencyObject obj)
        {
            return (IList)obj.GetValue(BindableSelectedItemsProperty);
        }

        public static void SetBindableSelectedItems(DependencyObject obj, IList value)
        {
            obj.SetValue(BindableSelectedItemsProperty, value);
        }

        private static void OnBindableSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox listBox)
            {
                listBox.SelectionChanged -= ListBox_SelectionChanged;
                listBox.SelectionChanged += ListBox_SelectionChanged;
            }
        }

        private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                IList selectedItems = GetBindableSelectedItems(listBox);
                if (selectedItems != null)
                {
                    selectedItems.Clear();
                    foreach (var item in listBox.SelectedItems)
                    {
                        selectedItems.Add(item);
                    }
                }
            }
        }
    }
}
