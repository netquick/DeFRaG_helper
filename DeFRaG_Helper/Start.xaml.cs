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
    /// Interaction logic for Start.xaml
    /// </summary>
    public partial class Start : Page
    {


        private static Start instance;

        public static Start Instance
        {
            get
            {
                if (instance == null)
                    instance = new Start();
                return instance;
            }
        }
        public Start()
        {
            InitializeComponent();
            //this.DataContext = MapViewModel.Instance; // Assuming Singleton pattern
            //if (MapViewModel.Instance.IsDataLoaded)
            //{
            //    // If data is already loaded, set DataContext immediately
            //    this.DataContext = MapViewModel.Instance;
            //}
            //else
            //{
            //    // Subscribe to the DataLoaded event
            //    MapViewModel.Instance.SubscribeToDataLoaded(MapViewModel_DataLoaded);
            //}

        }

        //private void MapViewModel_DataLoaded(object sender, EventArgs e)
        //{
        //    // Use the public method to unsubscribe from the DataLoaded event
        //    MapViewModel.Instance.UnsubscribeFromDataLoaded(MapViewModel_DataLoaded);

        //    // Set DataContext on the UI thread
        //    Dispatcher.Invoke(() =>
        //    {
        //        this.DataContext = MapViewModel.Instance;
        //    });
        //}


    }

    //Treehelper class f

}
