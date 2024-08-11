using DeFRaG_Helper.Converters;
using DeFRaG_Helper.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            mapHistoryManager = MapHistoryManager.Instance; // Correctly initialize the class-level field
        }

        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;

            // Switch on the content of lblAction
            switch (lblAction.Content)
            {
                case "Play Game":
                    // Start the oDFe.x64.exe in GameDirectoryPath from App.config
                    System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\oDFe.x64.exe", "+set fs_game defrag +df_promode " + mainWindow.GetPhysicsSetting());
                    break;
                case "Random Map":
                    // Check maps viewmodel for random map, containing physics according to chkPhysics in Start.xaml where 1 = vq3, 2 = cpma, 3 = vq3 and cpma
                    // Start the oDFe.x64.exe in GameDirectoryPath from App.config with the random map
                    PlayRandomMap();
                    break;
                case "Last Played":
                    // Get the last played map ID from the mapHistoryManager    
                    var lastPlayedMapId = await mapHistoryManager.GetLastPlayedMapIdAsync();
                    if (lastPlayedMapId.HasValue)
                    {
                        // Get the map name from the MapViewModel using the map ID
                        var mapViewModel = await MapViewModel.GetInstanceAsync();
                        var lastPlayedMap = await mapViewModel.GetMapByIdAsync(lastPlayedMapId.Value);
                        if (lastPlayedMap != null)
                        {
                            // Get the map name without extension
                            var mapNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(lastPlayedMap.Mapname);

                            // Start the oDFe.x64.exe in GameDirectoryPath from App.config with the last played map
                            System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\oDFe.x64.exe", $"+set fs_game defrag +df_promode {mainWindow.GetPhysicsSetting()} +map {mapNameWithoutExtension}");
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void DropdownButton_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = new ContextMenu();

            // Create and add menu items with icons
            MenuItem option1 = new MenuItem() { Header = "Play Game" };
            MenuItem option2 = new MenuItem() { Header = "Random Map" };
            MenuItem option3 = new MenuItem() { Header = "Last Played" };

            // Set icons for the menu items using the same source
            string iconSource = (string)FindResource("IconSource");
            var iconConverter = (DynamicSvgConverter)FindResource("DynamicSvgConverter");

            option1.Icon = new Image
            {
                Source = iconConverter.Convert(iconSource, typeof(ImageSource), null, null) as ImageSource,
                Width = 18, // Adjusted icon size
                Height = 18 // Adjusted icon size
            };
            option2.Icon = new Image
            {
                Source = iconConverter.Convert(iconSource, typeof(ImageSource), null, null) as ImageSource,
                Width = 18, // Adjusted icon size
                Height = 18 // Adjusted icon size
            };
            option3.Icon = new Image
            {
                Source = iconConverter.Convert(iconSource, typeof(ImageSource), null, null) as ImageSource,
                Width = 18, // Adjusted icon size
                Height = 18 // Adjusted icon size
            };

            menu.Items.Add(option1);
            menu.Items.Add(option2);
            menu.Items.Add(option3);

            // Add click event handlers for the menu items
            option1.Click += MenuItem_Click;
            option2.Click += MenuItem_Click;
            option3.Click += MenuItem_Click;

            // Set the placement of the ContextMenu
            menu.PlacementTarget = this; // The UserControl as the target
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom; // Place it at the bottom
            menu.HorizontalOffset = 0; // Align it horizontally
            menu.VerticalOffset = 0; // Optional: Adjust vertical offset if needed

            // Match the width of the ContextMenu with the DropDownButton
            menu.MinWidth = this.ActualWidth;
            menu.IsOpen = true;
        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Cast the sender back to a MenuItem
            MenuItem clickedItem = sender as MenuItem;

            if (clickedItem != null)
            {
                lblAction.Content = clickedItem.Header.ToString();
                AppConfig.ButtonState = clickedItem.Header.ToString(); // Update AppConfig with the new state
                await AppConfig.SaveConfigurationAsync(); // Save the updated configuration

                // Trigger the action associated with the new state
                ActionButton_Click(this, new RoutedEventArgs());
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

                var matchingMaps = viewModel.Maps
                    .Where(m => (m.Physics == physicsSetting || m.Physics == 0 || m.Physics == 3) && m.GameType == "Defrag")
                    .ToList();

                if (matchingMaps.Any())
                {
                    var random = new Random();
                    var randomMap = matchingMaps[random.Next(matchingMaps.Count)];

                    MessageHelper.ShowMessage($"Random map: {randomMap.Mapname} out of {matchingMaps.Count}");

                    //we need check if the map is downloaded and installed, if not, we will install it
                    await MapInstaller.InstallMap(randomMap);
                    mapHistoryManager = MapHistoryManager.Instance; ;

                    await mapHistoryManager.AddLastPlayedMapAsync(randomMap.Id, "Random");

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
