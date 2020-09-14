using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace SaveConverter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string Name = "GTA3 Mobile Save Converter";
        public const string Description = "Convert GTA3 Android saves to iOS and vice-versa.";
        public const string Author = "Wes Hampson";
        public static string Copyright => $"(C) 2020 {Author}";

        public static string Version => Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Gxt.TheText.Load(@"pack://application:,,,/Resources/american.gxt");

            MainWindow = new MainWindow();
            MainWindow.Show();
        }
    }
}
