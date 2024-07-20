﻿using DeFRaG_Helper.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DeFRaG_Helper
{
    /// <summary>
    /// Interaction logic for HighLightCard.xaml
    /// </summary>
    public partial class HighLightCard : UserControl
    {
        public HighLightCard()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MapProperty = DependencyProperty.Register(
       "Map", typeof(Map), typeof(HighLightCard), new PropertyMetadata(null, OnMapPropertyChanged));

        public Map Map
        {
            get { return (Map)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }

        private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as HighLightCard;
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
                MapViewModel.GetInstanceAsync().Result.SelectedMap = Map;
                PlayMap();

            }

        }
        private void PlayMap()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            int physicsSetting = mainWindow.GetPhysicsSetting(); // method in MainWindow
            System.Diagnostics.Process.Start(AppConfig.GameDirectoryPath + "\\oDFe.x64.exe", $"+set fs_game defrag +df_promode {physicsSetting} +map {System.IO.Path.GetFileNameWithoutExtension(Map.Mapname)}");

        }
        private async void FavoriteCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var map = checkBox.DataContext as Map;
            if (map != null)
            {
                map.IsFavorite = 1;
                var mapViewModel = await MapViewModel.GetInstanceAsync();

                await mapViewModel.UpdateFavoriteStateAsync(map);

            }
        }

        private async void FavoriteCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var map = checkBox.DataContext as Map;
            if (map != null)
            {
                map.IsFavorite = 0;
                var mapViewModel = await MapViewModel.GetInstanceAsync();

                await mapViewModel.UpdateFavoriteStateAsync(map);
            }
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
