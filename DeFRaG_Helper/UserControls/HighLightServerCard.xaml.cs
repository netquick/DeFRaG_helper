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
    /// Interaction logic for HighLightServerCard.xaml
    /// </summary>
    public partial class HighLightServerCard : UserControl
    {
        public HighLightServerCard()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ServerProperty = DependencyProperty.Register(
            "Server", typeof(ServerNode), typeof(HighLightServerCard), new PropertyMetadata(null, OnServerPropertyChanged));


        public ServerNode Server
        {
            get { return (ServerNode)GetValue(ServerProperty); }
            set { SetValue(ServerProperty, value); }
        }
        private static void OnServerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as HighLightServerCard;
            if (control != null)
            {
                control.DataContext = e.NewValue;
            }
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
