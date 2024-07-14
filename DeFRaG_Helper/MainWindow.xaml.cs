using DeFRaG_Helper.ViewModels;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Navigation;
using WPFFolderBrowser;



namespace DeFRaG_Helper
{
    public partial class MainWindow : Window
    {
        private System.Timers.Timer hideProgressBarTimer;
        private bool isFirstNavigationToMaps = true;
        private Page lastNavigatedPage = null;


        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }
        private static MainWindow _instance;

        public static MainWindow Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Application.Current.MainWindow as MainWindow;
                return _instance;
                SimpleLogger.Log("MainWindow Instance");
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            SimpleLogger.Log("MainWindow Constructor");
            MainFrame.Navigated += MainFrame_Navigated;

            this.SourceInitialized += MainWindow_SourceInitialized;
            SimpleLogger.Log("MainWindow SourceInitialized");
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
            NavigationListView.SelectionChanged += NavigationListView_SelectionChanged;
            AppConfig.OnRequestGameDirectory += RequestGameDirectoryAsync;

            // Initialize the timer with a 2-second interval
            hideProgressBarTimer = new System.Timers.Timer(2000);
            hideProgressBarTimer.Elapsed += HideProgressBarTimer_Elapsed;
            hideProgressBarTimer.AutoReset = false; // Ensure the timer runs only once per start
        }
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            ApplyFilterBasedOnPageAsync(e.Content);
        }
        private async Task<string> RequestGameDirectoryAsync()
        {
            // Assuming you have a method to show a dialog and return the selected path
            string selectedPath = await ShowDirectorySelectionDialogAsync();
            return selectedPath;
        }
        public static async Task<string> ShowDirectorySelectionDialogAsync()
        {
            string folderPath = string.Empty;
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new WPFFolderBrowserDialog
                {
                    Title = "Select a folder" // You can customize the dialog title here
                };
                var result = dialog.ShowDialog();
                if (result == true) // Check if the dialog was accepted
                {
                    folderPath = dialog.FileName; // Get the selected folder path
                }
            });
            return folderPath;
        }
        private async void ApplyFilterBasedOnPageAsync(object navigatedContent)
        {
            try
            {
                var serverViewModel = await ServerViewModel.GetInstanceAsync();
                var mapViewModel = await MapViewModel.GetInstanceAsync(); // Assuming this is accessible

                if (navigatedContent is Start)
                {
                    // Apply the filter to show only servers with CurrentPlayers > 0
                    serverViewModel.ServersView.Filter = item =>
                    {
                        if (item is ServerNode server)
                        {
                            return server.CurrentPlayers > 0;
                        }
                        return false;
                    };

                    // Load last played map IDs from MapHistoryManager and apply filter for maps
                    var mapHistoryManager = MapHistoryManager.GetInstance("DeFRaG_Helper");
                    List<int> lastPlayedMapIds = mapHistoryManager.GetLastPlayedMaps(); // This method is now synchronous
                    // Show only maps with IDs in the lastPlayedMapIds list, show in reversed order

                }
                else if (navigatedContent is Server)
                {
                    // Remove the filter to show all servers
                    serverViewModel.ServersView.Filter = null;
                }
                else if (navigatedContent is Maps)
                {
                    // Clear filters when navigating to Maps page
                    //Maps.Instance.ClearFilters();
    
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error applying filter: {ex.Message}");
                // Handle exceptions or log the error
            }
        }




        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MessagingService.Subscribe(ShowMessage);
            chkPhysics.IsChecked = AppConfig.PhysicsSetting == "VQ3";
            lblPhysics.Content = AppConfig.PhysicsSetting;
            await AppConfig.SaveConfigurationAsync(); // Save the updated configuration
            CheckGameInstall.StartChecks();


            await CheckAndInstallPreviewPictures();

        }

        private async Task CheckAndInstallPreviewPictures()
        {
            //get appdata path 
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appDataFolder = System.IO.Path.Combine(appDataPath, "DeFRaG_Helper");
            string previewPicturesPath = System.IO.Path.Combine(appDataFolder, "PreviewImages");
            if (!System.IO.Directory.Exists(previewPicturesPath))
            {
            SimpleLogger.Log("Images not found, downloading");
                //download images from https://dl.netquick.ch/PreviewImages.zip using Downloader class
                string url = "https://dl.netquick.ch/PreviewImages.zip";  
                string zipPath = System.IO.Path.Combine(appDataFolder, "PreviewImages.zip");
                var progressHandler = new Progress<double>(value =>
                {
                    UpdateProgressBar(value);
                });
                IProgress<double> progress = progressHandler;
                ShowMessage("Downloading images");
                await Downloader.DownloadFileAsync(url, zipPath, progress);
                ShowMessage("Images downloaded, unpacking");
                await Downloader.UnpackFile(zipPath, appDataFolder, progress);
                SimpleLogger.Log("Images downloaded and extracted");


            }
            else
            {
                //check if images are up to date
            }

        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            MessagingService.Unsubscribe(ShowMessage);
        }
        private void HideProgressBarTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                progressBar.Visibility = Visibility.Hidden;
            });
        }
        public void LoadNavigationBar()
        {
            NavigationListView.ItemsSource = new List<NavigationItem>
{
                new NavigationItem { Icon = "Icons/home.svg", Text = "Home", IsActive = true },
                new NavigationItem { Icon = "Icons/map.svg", Text = "Maps" },
                new NavigationItem { Icon = "Icons/globe.svg", Text = "Server" },
                new NavigationItem { Icon = "Icons/slideshow.svg", Text = "Demos" },
                new NavigationItem { Icon = "Icons/settings.svg", Text = "Settings" },
                // Add other items here
            };
            if (AppConfig.MenuState == "Collapsed")
            {
                SidebarColumn.Width = new GridLength(70); // Adjust to your collapsed width
                // Assuming you have a way to access the toggle button, e.g., a field named toggleButton
                toggleButton.Content = ">"; // Or your icon for collapsed state
                isSidebarCollapsed = true;
            }
            else // Default to expanded if not explicitly collapsed
            {
                SidebarColumn.Width = new GridLength(240); // Adjust to your expanded width
                toggleButton.Content = "<"; // Or your icon for expanded state
                isSidebarCollapsed = false;
            }
            MainFrame.Navigate(Start.Instance);

        }

        public void ShowProgressBar()
        {
            progressBar.Visibility = Visibility.Visible;
        }
        public void HideProgressBar()
        {
            progressBar.Visibility = Visibility.Hidden;
        }
        public void UpdateProgressBar(double value)
        {
            Debug.WriteLine("Updating progress bar: " + value);
            if (progressBar.Visibility != Visibility.Visible)
            {
                progressBar.Visibility = Visibility.Visible;
            }
            progressBar.Value = value;

            // Reset and start the timer every time progress is updated
            if (hideProgressBarTimer != null)
            {
                hideProgressBarTimer.Stop(); // Stop the timer if it's already running
                hideProgressBarTimer.Start(); // Restart the timer
            }
        }


        public int GetPhysicsSetting ()
        {
            // get state of chkPhysics checkbox and return the corresponding value for physics setting. 2 for CPM, 1 for VQ3
            return chkPhysics.IsChecked == true ? 2 : 1;

        }

        private void PhysicsMode_Changed(object sender, RoutedEventArgs e)
        {
            // Implementation logic here
            // Example: Toggle between physics modes based on the CheckBox's state
            var checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                AppConfig.PhysicsSetting = checkBox.IsChecked == true ? "VQ3" : "CPM";
                AppConfig.SaveConfigurationAsync(); // Save the updated configuration
                lblPhysics.Content = AppConfig.PhysicsSetting;
            }
        }
        public void ShowMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                // Create the message bubble as a Border with a TextBlock inside
                var messageBubble = new Border
                {
                    Background = new SolidColorBrush(Colors.Black),
                    CornerRadius = new CornerRadius(5),
                    Margin = new Thickness(5),
                    Padding = new Thickness(10),
                    Child = new TextBlock
                    {
                        Text = message,
                        Foreground = new SolidColorBrush(Colors.White),
                    }
                };

                // Add the message bubble to the StackPanel
                MessageHost.Children.Add(messageBubble);

                // Set a timer to remove the message after 5 seconds
                var timer = new System.Timers.Timer(5000);
                timer.Elapsed += (sender, e) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageHost.Children.Remove(messageBubble);
                        timer.Stop();
                    });
                };
                timer.Start();
            });
            SimpleLogger.Log(message);
        }

        private Start startPage = new Start();
        private Maps mapsPage = new Maps();
        private Settings settingsPage = new Settings();
        private Demos demosPage = new Demos();
        private Server serverPage = new Server();

        // Define other pages similarly
        private void NavigationListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavigationListView.SelectedItem is NavigationItem selectedItem)
            {
                // Reset IsActive for all items
                foreach (NavigationItem item in NavigationListView.Items)
                {
                    item.IsActive = false;
                }

                // Set the selected item as active
                selectedItem.IsActive = true;
                Page navigateToPage = null;

                // Navigate based on the selected item's Text property
                switch (selectedItem.Text)
                {
                    case "Home":
                        navigateToPage = startPage;
                        break;
                    case "Maps":
                        navigateToPage = mapsPage;
                        break;
                    case "Server":
                        navigateToPage = serverPage;
                        break;
                    case "Demos":
                            navigateToPage = demosPage; 
                        break;
                    case "Settings":
                        navigateToPage = settingsPage;
                        break;
                }

                // Check if we're navigating to a different page
                if (navigateToPage != null && navigateToPage != lastNavigatedPage)
                {
                    MainFrame.Navigate(navigateToPage);
                    lastNavigatedPage = navigateToPage; // Update the last navigated page
                }
            }
        }



        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            int isEnabled = 0;
            DwmIsCompositionEnabled(ref isEnabled);

            if (isEnabled == 1)
            {
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                MARGINS margins = new MARGINS { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };

                // Extend the frame across the entire window
                DwmExtendFrameIntoClientArea(hWnd, ref margins);
            }
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allows the window to be dragged around by the custom title bar
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) // Check if it's a double-click
            {
                // Toggle between maximized and normal window states
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                }
                else if (this.WindowState == WindowState.Normal)
                {
                    this.WindowState = WindowState.Maximized;
                }
            }
            else // Single click
            {
                // Existing logic to drag the window
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            }
        }



        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                searchBar.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Input);
            Debug.WriteLine("Got focus");
        }

        private void OnRightDragDelta(object sender, DragDeltaEventArgs e)
        {
            double newWidth = this.Width + e.HorizontalChange;
            if (this.Width > this.MinWidth)
            {
                this.Width = newWidth;
            }
        }

        private void OnBottomDragDelta(object sender, DragDeltaEventArgs e)
        {
            double newHeight = this.Height + e.VerticalChange;
            if (this.Height > this.MinHeight)
            {
                this.Height = newHeight;
            }
        }
        private void OnLeftDragDelta(object sender, DragDeltaEventArgs e)
        {
            double newWidth = this.Width - e.HorizontalChange;
            if (newWidth > this.MinWidth)
            {
                this.Width = newWidth;
                this.Left += e.HorizontalChange;
            }
        }

        private void OnTopDragDelta(object sender, DragDeltaEventArgs e)
        {
            double newHeight = this.Height - e.VerticalChange;
            if (newHeight > this.MinHeight)
            {
                this.Height = newHeight;
                this.Top += e.VerticalChange;
            }
        }
        private void OnBottomRightDragDelta(object sender, DragDeltaEventArgs e)
        {
            OnRightDragDelta(sender, e);
            OnBottomDragDelta(sender, e);
        }
        private bool isSidebarCollapsed = false; // Tracks the sidebar state

        private void ToggleSidebar(object sender, RoutedEventArgs e)
        {
            if (isSidebarCollapsed)
            {
                // Expand the sidebar
                SidebarColumn.Width = new GridLength(240); // Original width
                ((Button)sender).Content = "<"; // Adjust content as needed
                isSidebarCollapsed = false;
                AppConfig.MenuState = "Expanded";
            }
            else
            {
                // Collapse the sidebar
                SidebarColumn.Width = new GridLength(70); // Minimal width for collapsed state
                ((Button)sender).Content = ">"; // Adjust content as needed
                isSidebarCollapsed = true;
                AppConfig.MenuState = "Collapsed";
            }
            // Save the new state asynchronously
            AppConfig.SaveConfigurationAsync().ConfigureAwait(false);
        }


        private async void Srv_Click(object sender, RoutedEventArgs e)
        {
            Quake3ServerQuery query = new Quake3ServerQuery("83.243.73.220", 27961);
            var (Success, Response) = await query.QueryServerAsync();
            if (Success)
            {
                MessageBox.Show($"Query Successful: {Response}");
            }
            else
            {
                MessageBox.Show($"Query Failed: {Response}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
