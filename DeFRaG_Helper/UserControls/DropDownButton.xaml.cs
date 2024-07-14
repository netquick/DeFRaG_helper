using DeFRaG_Helper.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeFRaG_Helper
{
    /// <summary>
    /// Interaction logic for DropDownButton.xaml
    /// </summary>
    public partial class DropDownButton : UserControl
    {
        private MapHistoryManager mapHistoryManager;
        public delegate void MapPlayedEventHandler(object sender, EventArgs e);
        public event MapPlayedEventHandler MapPlayed;

        protected virtual void OnMapPlayed()
        {
            MapPlayed?.Invoke(this, EventArgs.Empty);
        }
        public DropDownButton()
        {
            InitializeComponent();
            lblAction.Content = AppConfig.ButtonState;
            var mapHistoryManager = MapHistoryManager.GetInstance("DeFRaG_Helper");

        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;

            //switch on the content of lblPlay
            switch (lblAction.Content)
            {
                case "Play Game":
                    //start the oDFe.x64.exe in GameDirectoryPath from App.config
                    System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\oDFe.x64.exe", "+set fs_game defrag +df_promode " + mainWindow.GetPhysicsSetting());
                    break;
                case "Random Map":
                    //check maps viewmodel for random map, containing physics according to chkPhysics in Start.xaml where 1 = vq3, 2 = cpma, 3 = vq3 and cpma
                    //start the oDFe.x64.exe in GameDirectoryPath from App.config with the random map


                    PlayRandomMap();

                    break;
                default:
                    break;
            }
           



           //App.Current.Dispatcher.Invoke(() => MainWindow.Instance.ShowMessage("Play"));


        }

        private void DropdownButton_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = new ContextMenu();

            // Create and add menu items
            MenuItem option1 = new MenuItem() { Header = "Play Game" };
            MenuItem option2 = new MenuItem() { Header = "Random Map" };
            menu.Items.Add(option1);
            menu.Items.Add(option2);

            // Add click event handlers for the menu items
            option1.Click += MenuItem_Click;
            option2.Click += MenuItem_Click;

            // Set the placement of the ContextMenu
            menu.PlacementTarget = this; // The UserControl as the target
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom; // Place it at the bottom
            menu.HorizontalOffset = 0; // Align it horizontally
            menu.VerticalOffset = 0; // Optional: Adjust vertical offset if needed

            // Match the width of the ContextMenu with the DropDownButton
            menu.MinWidth = this.ActualWidth;
            menu.IsOpen = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Cast the sender back to a MenuItem
            MenuItem clickedItem = sender as MenuItem;

            if (clickedItem != null)
            {
                lblAction.Content = clickedItem.Header.ToString();
                AppConfig.ButtonState = clickedItem.Header.ToString(); // Update AppConfig with the new state
                AppConfig.SaveConfigurationAsync(); // Save the updated configuration
            }
        }

        private async void PlayRandomMap()
        {
            // Ensure this method is called from an event that can handle async operations
            var viewModel = await MapViewModel.GetInstanceAsync();
            var mainWindow = Application.Current.MainWindow as MainWindow;

            if (viewModel != null && mainWindow != null)
            {
                // Assuming chkPhysics is accessible or you have a method in MainWindow to get its value
                int physicsSetting = mainWindow.GetPhysicsSetting(); // method in MainWindow
                //from the physics setting, we need to get maps with the same physics setting or 0 or 3 to find a random map

                var matchingMaps = viewModel.Maps.Where(m => m.Physics == physicsSetting || m.Physics == 0 || m.Physics == 3).ToList();

                if (matchingMaps.Any())
                {
                    var random = new Random();
                    var randomMap = matchingMaps[random.Next(matchingMaps.Count)];

                    App.Current.Dispatcher.Invoke(() => MainWindow.Instance.ShowMessage($"Random map: {randomMap.Mapname} out of {matchingMaps.Count}"));

                    //we need check if the map is downloaded and installed, if not, we will install it
                    await MapInstaller.InstallMap(randomMap);
                    mapHistoryManager = MapHistoryManager.GetInstance("DeFRaG_Helper");

                    await mapHistoryManager.UpdateLastPlayedMapsAsync(randomMap.Id);


                    System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\oDFe.x64.exe", $"+set fs_game defrag +df_promode {physicsSetting} +map {System.IO.Path.GetFileNameWithoutExtension(randomMap.Mapname)}"); 
                    Debug.WriteLine($"Random map: {randomMap.Mapname} out of {matchingMaps.Count}");
                }
            }
            OnMapPlayed();
        }
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Check if the new width is greater than 100px
            if (e.NewSize.Width > 100)
            {
                // Show the button
                DropdownButton.Visibility = Visibility.Visible;
            }
            else
            {
                // Hide the button
                DropdownButton.Visibility = Visibility.Collapsed;
            }
        }


    }

}
