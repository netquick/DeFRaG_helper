using System.Windows.Controls;
using System.Windows;
using DeFRaG_Helper.ViewModels;

namespace DeFRaG_Helper.UserControls
{
    public partial class TagManager : UserControl
    {
        private Window _associatedWindow;
        private bool _isInternalClick = false;

        public TagManager(TagManagerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Handle Loaded event to get the associated window
            this.Loaded += OnLoaded;
            // Handle LostFocus event
            this.LostFocus += OnLostFocus;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _associatedWindow = Window.GetWindow(this);
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (_associatedWindow != null && _associatedWindow.IsVisible && !_isInternalClick)
            {
                _associatedWindow.Close();
            }

            // Reset the internal click flag
            _isInternalClick = false;

            // Bring the main window to the foreground
            Application.Current.MainWindow.Activate();
        }

        private void CheckBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Set the flag to indicate an internal click
            _isInternalClick = true;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (DataContext is TagManagerViewModel viewModel && sender is CheckBox checkBox && checkBox.DataContext is TagTextItem tag)
            {
                viewModel.TagCheckedCommand.Execute(tag);
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is TagManagerViewModel viewModel && sender is CheckBox checkBox && checkBox.DataContext is TagTextItem tag)
            {
                viewModel.TagUncheckedCommand.Execute(tag);
            }
        }
    }
}
