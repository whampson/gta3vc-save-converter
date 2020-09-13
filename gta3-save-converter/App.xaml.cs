using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SaveConverter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Gxt.TheText.Load(@"pack://application:,,,/Resources/american.gxt");

            MainWindow = new MainWindow();
            MainWindow.Show();
        }
    }
}
