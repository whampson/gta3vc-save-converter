using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;

namespace SaveConverter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string Name = "GTA3/VC Mobile Save Converter";
        public const string Description = "Convert GTA3 Android saves to iOS and vice-versa.";
        public const string Author = "Wes Hampson (thehambone)";
        public static string Copyright => $"(C) 2020 {Author}";

        public static string Version => Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;

        public static Dictionary<string, string> GxtGTA3 { get; set; }
        public static Dictionary<string, Dictionary<string, string>> GxtVC { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            GxtGTA3 = GxtLoader.LoadGTA3(LoadResource("GTA3/american.gxt"));
            GxtVC = GxtLoader.LoadVC(LoadResource("VC/american.gxt"));

            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        public static byte[] LoadResource(string resourceName)
        {
            return LoadResource(new Uri($"pack://application:,,,/Resources/{resourceName}"));
        }

        public static byte[] LoadResource(Uri resourceUri)
        {
            using MemoryStream m = new MemoryStream();
            GetResourceStream(resourceUri).Stream.CopyTo(m);
            return m.ToArray();
        }
    }
}
