using System.Windows.Controls;
using System.Windows;
using DeFRaG_Helper.ViewModels;

namespace DeFRaG_Helper.UserControls
{
    public partial class TagManager : UserControl
    {
        public bool IsInternalClick { get; set; } = false;

        public TagManager(TagManagerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void CheckBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Set the flag to indicate an internal click
            IsInternalClick = true;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (DataContext is TagManagerViewModel viewModel && sender is CheckBox checkBox && checkBox.DataContext is TagTextItem tag)
            {
                viewModel.TagCheckedCommand.Execute(tag);
                IsInternalClick = false; // Reset the flag
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is TagManagerViewModel viewModel && sender is CheckBox checkBox && checkBox.DataContext is TagTextItem tag)
            {
                viewModel.TagUncheckedCommand.Execute(tag);
                IsInternalClick = false; // Reset the flag
            }
        }
    }
}
