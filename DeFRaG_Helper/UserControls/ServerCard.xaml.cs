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
using System.Diagnostics;

namespace DeFRaG_Helper
{
    /// <summary>
    /// Interaction logic for ServerCard.xaml
    /// </summary>
    public partial class ServerCard : UserControl
    {
        public ServerCard()
        {
            InitializeComponent();

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
    }
}
