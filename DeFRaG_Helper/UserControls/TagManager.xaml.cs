using System.Windows.Controls;
using System.Windows;
using DeFRaG_Helper.ViewModels;

namespace DeFRaG_Helper.UserControls
{
    public partial class TagManager : UserControl
    {
        public TagManager(TagManagerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.RequestClose += (sender, e) =>
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.Close();
                }
            };
        }
    }
}
