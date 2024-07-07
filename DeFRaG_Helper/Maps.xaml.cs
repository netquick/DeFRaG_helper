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

namespace DeFRaG_Helper
{
    /// <summary>
    /// Interaction logic for Maps.xaml
    /// </summary>
    public partial class Maps : Page
    {
        private static Maps instance;

        public static Maps Instance
        {
            get
            {
                if (instance == null)
                    instance = new Maps();
                return instance;
            }
        }
        public Maps()
        {
            InitializeComponent();
            var _ = InitializeDataContextAsync(); // Discard the task since we can't await in the constructor

        }
        private async Task InitializeDataContextAsync()
        {
            this.DataContext = await MapViewModel.GetInstanceAsync();
        }

        //method to set favorite map
    }
}
