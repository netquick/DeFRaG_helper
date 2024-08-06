using DeFRaG_Helper.Objects;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DeFRaG_Helper.Windows
{
    public partial class CustomBrowser : Window
    {
        public ObservableCollection<DirectoryItem> Items { get; set; }
        private DirectoryInfo currentDirectory;
        public string SelectedFolderPath { get; private set; }
        private DispatcherTimer clickTimer;
        private const int DoubleClickTime = 300; // Time in milliseconds
        private bool isDoubleClick = false;
        private readonly string? initialPath;

        public CustomBrowser(string? initialPath = null)
        {
            InitializeComponent();
            Items = new ObservableCollection<DirectoryItem>();
            FolderListView.ItemsSource = Items;

            this.initialPath = initialPath;

            LoadDrives(initialPath);

            if (!string.IsNullOrEmpty(initialPath) && Directory.Exists(initialPath))
            {
                currentDirectory = new DirectoryInfo(initialPath);
                LoadDirectory(currentDirectory);
            }
            else
            {
                currentDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                LoadDirectory(currentDirectory);
            }

            clickTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(DoubleClickTime)
            };
            clickTimer.Tick += ClickTimer_Tick;

            // Initialize non-nullable fields
            SelectedFolderPath = string.Empty;
        }

        private void LoadDrives(string? initialPath = null)
        {
            cmbDrive.Items.Clear();
            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                cmbDrive.Items.Add(drive.Name);
            }

            if (!string.IsNullOrEmpty(initialPath))
            {
                var driveLetter = Path.GetPathRoot(initialPath);
                cmbDrive.SelectedItem = driveLetter;
            }
            else
            {
                cmbDrive.SelectedItem = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady)?.Name;
            }
        }

        private void LoadDirectory(DirectoryInfo directory)
        {
            Items.Clear();
            currentDirectory = directory;
            txtPath.Text = currentDirectory.FullName; // Update the path in the TextBox
            foreach (var dir in directory.GetDirectories())
            {
                Items.Add(new DirectoryItem(dir));
            }
        }

        private void FolderListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            isDoubleClick = true;
            clickTimer.Stop(); // Stop the timer if a double-click is detected
            if (FolderListView.SelectedItem is DirectoryItem directoryItem)
            {
                LoadDirectory(directoryItem.DirectoryInfo);
            }
        }

        private void FolderListView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (currentDirectory.Parent != null)
            {
                LoadDirectory(currentDirectory.Parent);
            }
        }

        private void cmbDrive_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDrive.SelectedItem != null)
            {
                var selectedDrive = cmbDrive.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(selectedDrive))
                {
                    LoadDirectory(new DirectoryInfo(selectedDrive));
                }
            }
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            if (FolderListView.SelectedItem is DirectoryItem selectedItem)
            {
                SelectedFolderPath = selectedItem.DirectoryInfo.FullName;
            }
            else
            {
                SelectedFolderPath = currentDirectory.FullName;
            }
            this.DialogResult = true;
            this.Close();
        }

        private void FolderListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) // Check if it's a double-click
            {
                isDoubleClick = true;
                clickTimer.Stop(); // Stop the timer if a double-click is detected
                if (FolderListView.SelectedItem is DirectoryItem directoryItem)
                {
                    LoadDirectory(directoryItem.DirectoryInfo);
                }
            }
            else
            {
                var item = ItemsControl.ContainerFromElement(FolderListView, e.OriginalSource as DependencyObject) as ListViewItem;
                if (item != null && item.IsSelected)
                {
                    item.IsSelected = false;
                    e.Handled = true; // Prevent further processing of the event
                }
                else
                {
                    isDoubleClick = false;
                    clickTimer.Start(); // Start the timer for single-click detection
                }
            }
        }

        private void ClickTimer_Tick(object? sender, EventArgs e)
        {
            clickTimer.Stop();
            if (!isDoubleClick)
            {
                // Handle single-click actions here if needed
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (textBlock != null)
            {
                var listViewItem = (ListViewItem)FolderListView.ItemContainerGenerator.ContainerFromItem(textBlock.DataContext);
                if (listViewItem != null)
                {
                    var textBox = FindVisualChild<TextBox>(listViewItem);
                    if (textBox != null)
                    {
                        textBox.Visibility = Visibility.Visible;
                        textBox.Focus();
                        textBox.SelectAll();
                    }
                }
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    var childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }

        //Method to implement window resizing functionality on doubleclick
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) // Check if it's a double-click
            {
                // Toggle between maximized and normal window states
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                }
            }
            else if (e.ButtonState == MouseButtonState.Pressed) // Check if the left mouse button is pressed
            {
                this.DragMove(); // Enable window dragging
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

