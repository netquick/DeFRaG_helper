﻿using DeFRaG_Helper.ViewModels;
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

namespace DeFRaG_Helper.UserControls
{
    /// <summary>
    /// Interaction logic for HighlightCardBig.xaml
    /// </summary>
    public partial class HighlightCardBig : UserControl
    {
        public HighlightCardBig()
        {
            InitializeComponent();
        }


        public static readonly DependencyProperty MapProperty = DependencyProperty.Register(
       "Map", typeof(Map), typeof(HighlightCardBig), new PropertyMetadata(null, OnMapPropertyChanged));

        public Map Map
        {
            get { return (Map)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }

        private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as HighlightCardBig;
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
            MainBorder.Background = new SolidColorBrush(Color.FromRgb(17, 17, 17)); // Example color
        }
    }
}
