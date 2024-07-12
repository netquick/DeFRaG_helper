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

            // Create a view for the Servers collection
            ICollectionView serversView = CollectionViewSource.GetDefaultView(viewModel.Servers);

            // Apply a filter to show only servers with CurrentPlayers >= 0
            serversView.Filter = ServerHasPlayers;

            // Apply a sort description to order the servers by CurrentPlayers
            serversView.SortDescriptions.Add(new SortDescription("CurrentPlayers", ListSortDirection.Descending));

            // Set the DataContext and ItemsSource
            this.DataContext = this;
            ServersItemsControl.ItemsSource = serversView;
        }

        private bool ServerHasPlayers(object item)
        {
            if (item is ServerNode serverNode) // Replace ServerNode with your actual server class
            {
                return serverNode.CurrentPlayers >= 0; // Show servers with CurrentPlayers >= 0
            }
            return false;
        }

    }
}
