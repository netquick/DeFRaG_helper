using DeFRaG_Helper.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

                    // Handle Deactivated event to close the window
                    window.Deactivated += (s, e) => window.Close();

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
