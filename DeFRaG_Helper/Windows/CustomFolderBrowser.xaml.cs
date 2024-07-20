using System.Windows;
using System.Windows.Controls;

namespace DeFRaG_Helper.Windows
{
    /// <summary>
    /// Interaction logic for CustomFolderBrowser.xaml
    /// </summary>
    public partial class CustomFolderBrowser : Window
    {
        public string SelectedFolderPath { get; private set; }

        public CustomFolderBrowser()
        {
            InitializeComponent();
            PopulateDrives();
        }



        private void PopulateDrives()
        {
            foreach (var drive in System.IO.DriveInfo.GetDrives().Where(d => d.DriveType == System.IO.DriveType.Fixed))
            {
                TreeViewItem driveItem = new TreeViewItem
                {
                    Header = drive.Name,
                    Tag = drive
                };
                driveItem.Items.Add(null); // Placeholder for lazy loading
                driveItem.Expanded += DriveItem_Expanded;
                treeView.Items.Add(driveItem);
            }
        }
        private void DriveItem_Expanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == null)
            {
                item.Items.Clear();
                var drive = (System.IO.DriveInfo)item.Tag;
                try
                {
                    foreach (var directory in drive.RootDirectory.GetDirectories())
                    {
                        TreeViewItem subItem = new TreeViewItem
                        {
                            Header = directory.Name,
                            Tag = directory
                        };
                        subItem.Items.Add(null); // Placeholder for lazy loading
                        subItem.Expanded += FolderItem_Expanded;
                        item.Items.Add(subItem);
                    }
                }
                catch { /* Handle access denied exceptions */ }
            }
        }
        private void FolderItem_Expanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;
            // Check if the item only contains the placeholder (null), indicating the subdirectories haven't been loaded yet
            if (item.Items.Count == 1 && item.Items[0] == null)
            {
                item.Items.Clear(); // Remove the placeholder
                var directoryInfo = (System.IO.DirectoryInfo)item.Tag;
                try
                {
                    foreach (var subDirectory in directoryInfo.GetDirectories())
                    {
                        TreeViewItem subItem = new TreeViewItem
                        {
                            Header = subDirectory.Name,
                            Tag = subDirectory
                        };
                        // Add a placeholder to indicate that this item can be expanded
                        subItem.Items.Add(null);
                        // Subscribe to the Expanded event for the new subItem
                        subItem.Expanded += FolderItem_Expanded;
                        item.Items.Add(subItem);
                    }
                }
                catch (System.UnauthorizedAccessException)
                {
                    // Handle the case where the application does not have permission to access the directory
                    item.Items.Add(new TreeViewItem { Header = "Access Denied" });
                }
                catch (Exception ex)
                {
                    // Handle other potential exceptions, such as a directory being deleted between the time it was queried and accessed
                    MessageBox.Show($"Error accessing directory: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Optional: Set DialogResult to false to indicate cancellation
            this.Close();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (treeView.SelectedItem is TreeViewItem selectedItem && selectedItem.Tag is System.IO.DirectoryInfo directoryInfo)
            {
                SelectedFolderPath = directoryInfo.FullName;
                this.DialogResult = true; // Optional: Set DialogResult to true to indicate successful selection
            }
            else
            {
                MessageBox.Show("Please select a folder.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            this.Close();
        }

    }
}
