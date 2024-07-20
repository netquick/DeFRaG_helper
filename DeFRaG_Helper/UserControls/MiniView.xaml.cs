using DeFRaG_Helper.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeFRaG_Helper.UserControls
{
    /// <summary>
    /// Interaction logic for MiniView.xaml
    /// </summary>
    public partial class MiniView : UserControl
    {
        public MiniView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MapProperty = DependencyProperty.Register(
       "Map", typeof(Map), typeof(MiniView), new PropertyMetadata(null, OnMapPropertyChanged));

        public Map Map
        {
            get { return (Map)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }
        private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MiniView; // Corrected from HighLightCard to MiniView
            if (control != null)
            {
                control.DataContext = e.NewValue;
            }
        }

        private async void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Cast the DataContext to a Map object
            var map = this.DataContext as Map;
            if (map != null)
            {
                // Assuming you have a way to access the MapViewModel instance
                var viewModel = MapViewModel.GetInstanceAsync().Result; // Note: Using .Result for simplicity; consider using async/await.
                viewModel.SelectedMap = map;
                await viewModel.UpdateConfigurationAsync(map);


            }
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Prevent event from bubbling up to parent elements
                e.Handled = true;
                var map = this.DataContext as Map;

                // Call your connection logic here
                PlayMap();
            }

        }


        private void PlayMap()
        {
            var map = this.DataContext as Map; // Correctly assigned to a local variable

            if (map == null)
            {
                MessageHelper.Log("Map data is not available.");
                return; // Exit the method if map is null
            }

            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
            {
                MessageHelper.Log("Main window is not accessible.");
                return; // Exit the method if mainWindow is null
            }

            int physicsSetting = mainWindow.GetPhysicsSetting(); // method in MainWindow
                                                                 // Use the local variable 'map' instead of the property 'Map'
            System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\oDFe.x64.exe", $"+set fs_game defrag +df_promode {physicsSetting} +map {System.IO.Path.GetFileNameWithoutExtension(map.Mapname)}");
        }



    }
}
