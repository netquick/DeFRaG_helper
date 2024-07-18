using DeFRaG_Helper.ViewModels;
using System;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeFRaG_Helper.UserControls
{
    /// <summary>
    /// Interaction logic for DemoCard.xaml
    /// </summary>
    public partial class DemoCard : UserControl
    {
        public DemoCard()
        {
            InitializeComponent();
        }



        private async void DemoCard_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Check if the actual item (not empty space) in the ListView was double-clicked
            var item = ((FrameworkElement)e.OriginalSource).DataContext as DemoItem;
            if (item != null)
            {
                //we need the mapname of the actual demo
                var mapName = item.Mapname;
                //now we have to check if the map is installed. We check "IsInstalled" property of the map
                var viewModel = MapViewModel.GetInstanceAsync().Result;
                MessageHelper.Log($"Checking for Mapname: {mapName}");
                var map = viewModel.Maps.FirstOrDefault(m => string.Equals(m.Mapname, mapName + ".bsp", StringComparison.OrdinalIgnoreCase));
                if (map != null)
                {
                    if (map.IsInstalled == 0)
                    {
                        await MapInstaller.InstallMap(map);
                        MessageHelper.Log($"Map {map.Mapname} installed.");
                    }
                    //Prepare the progress handler to update the UI
                    var progressHandler = new Progress<double>(value =>
                    {
                        App.Current.Dispatcher.Invoke(() => MainWindow.Instance.UpdateProgressBar(value));
                    });
                    //Prepare the link for the demo. It's contructed from http://95.31.6.66/~/api/get_file_list?uri=/demos/{mapName[0]}/{mapName}/" and the name of the demo
                    var demoLink = $"http://95.31.6.66/demos/{mapName[0]}/{mapName}/{item.Name}";

                    //check if there is a demo folder. If not, create it
                    if (!System.IO.Directory.Exists(AppConfig.GameDirectoryPath + "\\defrag\\demos"))
                    {
                        System.IO.Directory.CreateDirectory(AppConfig.GameDirectoryPath + "\\defrag\\demos");
                    }
                    //check if there is a demo folder for the map. If not, create it
                    if (!System.IO.Directory.Exists(AppConfig.GameDirectoryPath + $"\\defrag\\demos\\{mapName}"))
                    {
                        System.IO.Directory.CreateDirectory(AppConfig.GameDirectoryPath + $"\\defrag\\demos\\{mapName}");
                    }

                    //Download the demo to the demo folder
                    await Downloader.DownloadFileAsync(demoLink, AppConfig.GameDirectoryPath + $"\\defrag\\demos\\{mapName}\\{item.Name}", progressHandler);




                    //System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\oDFe.x64.exe", $"+demo {mapName}//{System.IO.Path.GetFilenameWithoutExtension(item.Name)}") ;
                    System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\oDFe.x64.exe", $"+demo {mapName}/{System.IO.Path.GetFileNameWithoutExtension(item.Name)}");


                }




                // Implement your double-click logic here
                // For example, navigate to a detail page or display a dialog
                ///MessageBox.Show($"Double-clicked on item: {item.Name}");


            }
        }
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            // Change to a slightly lighter or darker background color on hover
            MainBorder.Background = new SolidColorBrush(Color.FromRgb(62, 62, 62)); // Example color
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            // Revert to the original background color
            MainBorder.Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)); // Example color
        }
    }
}
