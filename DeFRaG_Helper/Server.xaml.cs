using DeFRaG_Helper.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for Server.xaml
    /// </summary>
    public partial class Server : Page
    {

        private static Server instance;

        public static Server Instance
        {
            get
            {
                if (instance == null)
                    instance = new Server();
                return instance;
            }
        }

        public Server()
        {
            InitializeComponent();
            var _ = InitializeDataContextAsync(); // Discard the task

        }
        private async Task InitializeDataContextAsync()
        {
            this.DataContext = await ServerViewModel.GetInstanceAsync();

        }

        public static List<(string Text, SolidColorBrush Color)> ParseQuakeColorCodes(string serverName)
        {
            var segments = new List<(string Text, SolidColorBrush Color)>();
            var colors = new Dictionary<char, SolidColorBrush>
                {
                    { '0', Brushes.Black },
                    { '1', Brushes.Red },
                    { '2', Brushes.Green },
                    { '3', Brushes.Yellow },
                    { '4', Brushes.Blue },
                    { '5', Brushes.Cyan },
                    { '6', Brushes.Magenta },
                    { '7', Brushes.White },
                    { '8', Brushes.Orange },
                    { '9', Brushes.Gray },
                    // Add more colors if needed
                };

            int lastIndex = 0;
            for (int i = 0; i < serverName.Length; i++)
            {
                if (serverName[i] == '^' && i + 1 < serverName.Length && colors.ContainsKey(serverName[i + 1]))
                {
                    if (i > lastIndex)
                    {
                        segments.Add((serverName.Substring(lastIndex, i - lastIndex), Brushes.White)); // Default color
                    }
                    lastIndex = i + 2; // Skip color code
                    i++; // Move past the color digit

                    if (i + 1 < serverName.Length)
                    {
                        int nextColorIndex = serverName.IndexOf('^', i + 1);
                        if (nextColorIndex == -1) nextColorIndex = serverName.Length;
                        segments.Add((serverName.Substring(i + 1, nextColorIndex - i - 1), colors[serverName[i]]));
                        i = nextColorIndex - 1;
                        lastIndex = nextColorIndex;
                    }
                }
            }

            // Add the last segment if there's any
            if (lastIndex < serverName.Length)
            {
                segments.Add((serverName.Substring(lastIndex), Brushes.White)); // Default color
            }

            return segments;
        }

    }

}
    


    

