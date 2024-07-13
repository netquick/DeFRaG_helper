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
using System.Windows.Threading;

namespace DeFRaG_Helper
{
    /// <summary>
    /// Interaction logic for Demos.xaml
    /// </summary>
    public partial class Demos : Page
    {
        private DispatcherTimer debounceTimer;


        private static Demos instance;

        public static Demos Instance
        {
            get
            {
                if (instance == null)
                    instance = new Demos();
                return instance;
            }
        }
        public Demos()
        {
            InitializeComponent();
            LoadViewModelAsync();  
            this.Loaded += DemoLoaded;
        }
        //method when demo loaded
        private void DemoLoaded(object sender, RoutedEventArgs e)
        {
            LoadDataAsync();

        }

        //Method to load the data for the selected map
        private async void LoadDataAsync()
        {
            var viewModel = await MapViewModel.GetInstanceAsync();
            var selectedMap = viewModel.SelectedMap;
            if (selectedMap != null)
            {
                //MapName without extension

                var demoLink = DemoParser.GetDemoLink(System.IO.Path.GetFileNameWithoutExtension(selectedMap.MapName));
                var demoItems = await DemoParser.GetDemoLinksAsync(demoLink);
                foreach (var item in demoItems)
                {
                    item.DecodeName();
                }
                lvDemos.ItemsSource = demoItems;
                txtMapSearch.Text = System.IO.Path.GetFileNameWithoutExtension(selectedMap.MapName);

            }
        }

        //method to load the demo data
        private async void LoadDemoDataAsync()
        {
            var viewModel = await MapViewModel.GetInstanceAsync();
            var selectedMap = viewModel.SelectedMap;
            if (selectedMap != null)
            {
                var demoLink = DemoParser.GetDemoLink(selectedMap.Name);
                var demoItems = await DemoParser.GetDemoLinksAsync(demoLink);
                foreach (var item in demoItems)
                {
                    item.DecodeName();
                }
                lvDemos.ItemsSource = demoItems;
            }
        }

        private async void TxtMapSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(txtMapSearch.Text))
                {
                    // Clear the ListView's ItemsSource
                    lvDemos.ItemsSource = null;
                }
                else
                {
                    LoadDemoDataAsync();
                }
            }
        }



        private async void LoadViewModelAsync()
        {
            var viewModel = await MapViewModel.GetInstanceAsync();
            this.DataContext = viewModel; // This sets the DataContext for the entire page
                                          // Alternatively, set the DataContext for just the ComboBox if needed:
                                          // cmbMap.DataContext = viewModel;
            var selectedMap = viewModel.SelectedMap; // Access the selected map

        }




        private void LvDemos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Check if the actual item (not empty space) in the ListView was double-clicked
            var item = ((FrameworkElement)e.OriginalSource).DataContext as DemoItem;
            if (item != null)
            {
                // Implement your double-click logic here
                // For example, navigate to a detail page or display a dialog
                MessageBox.Show($"Double-clicked on item: {item.Name}");
            }
        }

    }
}
