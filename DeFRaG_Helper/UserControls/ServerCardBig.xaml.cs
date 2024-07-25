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

namespace DeFRaG_Helper.UserControls
{
    /// <summary>
    /// Interaction logic for ServerCardBig.xaml
    /// </summary>
    public partial class ServerCardBig : UserControl
    {
        public ServerCardBig()
        {
            InitializeComponent();
            this.DataContextChanged += ServerCardBig_DataContextChanged;

        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Prevent event from bubbling up to parent elements
                e.Handled = true;

                // Call your connection logic here
                ConnectToServer();
            }

        }
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            ConnectToServer();
        }
        private void ConnectToServer()
        {
            //connect to quake server under the ip and port in the server cards corresponding textblocks

            if (DataContext is ServerNode serverNode)
            {
                // Execute the command
                System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\oDFe.x64.exe", $"+connect {serverNode.IP}:{serverNode.Port}");

            }


            // Your logic to initiate a connection to the server
            Debug.WriteLine($"Connecting to server at ");
            // Implement the actual connection logic here
        }
        public void LoadMapIcons(Map map)
        {
            if (DataContext is ServerNode serverNode)
            {
                serverNode.LoadMapIcons(map);
            }
        }
        private async void ServerCardBig_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ServerNode serverNode)
            {
                // Find the actual Map object using the MapViewModel instance
                var mapViewModel = await MapViewModel.GetInstanceAsync();

                var map = mapViewModel.GetMapByName(serverNode.Map);
                if (map != null)
                {
                    LoadMapIcons(map);
                }
                else
                {
                    Debug.WriteLine($"Map '{serverNode.Map}' not found.");
                }
            }
        }
    }
}
