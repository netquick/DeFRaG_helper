using System.Configuration;
using System.Data;
using System.Windows;

namespace DeFRaG_Helper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppConfig.LoadConfiguration();
            // Other startup logic...
        }
    }

}
