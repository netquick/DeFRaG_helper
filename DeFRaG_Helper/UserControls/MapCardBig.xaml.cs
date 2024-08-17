using DeFRaG_Helper.ViewModels;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DeFRaG_Helper.UserControls
{
    /// <summary>
    /// Interaction logic for MapCardBig.xaml
    /// </summary>
    public partial class MapCardBig : UserControl
    {
        public Map CurrentMap { get; set; }
        public static readonly DependencyProperty IconsProperty = DependencyProperty.Register(
            "Icons", typeof(IEnumerable), typeof(MapCardBig), new PropertyMetadata(null));

        public ICommand OpenTagManagerCommand { get; }


        public MapCardBig()
        {
            InitializeComponent();
            //DataContext = MapViewModel.GetInstanceAsync().Result;
            OpenTagManagerCommand = new RelayCommand(OpenTagManager);

        }



        private void OpenTagManager(object parameter)
        {
            if (parameter is Button triggerButton)
            {
                var map = triggerButton.DataContext as Map;
                if (map != null)
                {
                    var viewModel = new TagManagerViewModel(map);
                    var tagManager = new TagManager(viewModel);

                    var window = new Window
                    {
                        Content = tagManager,
                        Width = 400,
                        Height = 300,
                        Title = "Manage Tags",
                        WindowStyle = WindowStyle.None,
                        AllowsTransparency = true,
                        Background = new SolidColorBrush(Color.FromArgb(204, 17, 17, 17)) // 80% alpha and color #111
                    };

                    // Set the owner of the window to the main application window
                    window.Owner = Application.Current.MainWindow;

                    // Get the position of the button that triggered the popup
                    Point buttonPosition = triggerButton.PointToScreen(new Point(0, 0));
                    window.Left = buttonPosition.X;
                    window.Top = buttonPosition.Y;

                    // Handle the Deactivated event to close the popup
                    window.Deactivated += (s, e) =>
                    {
                        if (!tagManager.IsInternalClick)
                        {
                            window.Close();

                            // Bring the main window to the foreground
                            Application.Current.MainWindow.Activate();
                            var mainWindow = Application.Current.MainWindow;
                            mainWindow.Topmost = true;  // Set MainWindow as topmost
                            mainWindow.Topmost = false; // Revert MainWindow back to normal
                        }
                        else
                        {
                            // Reset the internal click flag
                            tagManager.IsInternalClick = false;
                        }
                    };

                    // Add fade-in animation
                    var fadeInAnimation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
                    window.BeginAnimation(Window.OpacityProperty, fadeInAnimation);

                    window.Show();
                }
            }
        }









        public IEnumerable Icons
        {
            get { return (IEnumerable)GetValue(IconsProperty); }
            set { SetValue(IconsProperty, value); }
        }
        private async void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var mapViewModel = await MapViewModel.GetInstanceAsync();
                CurrentMap = (sender as Border).DataContext as Map;
                if (mapViewModel != null && mapViewModel.PlayMapCommand.CanExecute(CurrentMap))
                {
                    mapViewModel.PlayMapCommand.Execute(CurrentMap);
                }
            }
        }
        private async void FavoriteCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var map = checkBox.DataContext as Map;
            if (map != null)
            {
                map.IsFavorite = 1;
                var mapViewModel = await MapViewModel.GetInstanceAsync();

                await mapViewModel.UpdateFavoriteStateAsync(map);

            }
        }

        private async void FavoriteCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var map = checkBox.DataContext as Map;
            if (map != null)
            {
                map.IsFavorite = 0;
                var mapViewModel = await MapViewModel.GetInstanceAsync();

                await mapViewModel.UpdateFavoriteStateAsync(map);
            }
        }
    }


}
