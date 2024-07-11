using DeFRaG_Helper.ViewModels;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

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
            var viewModel = await ServerViewModel.GetInstanceAsync();

            // Create a view for the Servers collection and apply a filter
            ICollectionView serversView = CollectionViewSource.GetDefaultView(viewModel.Servers);
            // serversView.Filter = ServerHasPlayers;
            this.DataContext = this;

            // Set the ItemsSource of your ItemsControl to the filtered view
            serversView.Filter = null;
            ServersItemsControl.ItemsSource = serversView;

        }

        private bool ServerHasPlayers(object item)
        {
            if (item is ServerNode serverNode) // Replace ServerNode with your actual server class
            {
                return serverNode.CurrentPlayers > 0; // Adjust CurrentPlayers to your actual property name
            }
            return false;
        }

    }
}
