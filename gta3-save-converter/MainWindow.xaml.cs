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
using WpfEssentials.Win32;

namespace SaveConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SaveFileConverter ViewModel
        {
            get { return (SaveFileConverter) DataContext; }
            set { DataContext = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.MessageBoxRequest += ViewModel_MessageBoxRequest;
            ViewModel.FileDialogRequest += ViewModel_FileDialogRequest;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.MessageBoxRequest -= ViewModel_MessageBoxRequest;
            ViewModel.FileDialogRequest -= ViewModel_FileDialogRequest;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 1)
                {
                    ViewModel.ShowError("Multiple files selected. Please select only one file.");
                    return;
                }

                ViewModel.CloseFile();
                ViewModel.OpenFile(files[0]);
            }
            else
            {
                ViewModel.ShowError("Data format not supported. Please select a file.");
                return;
            }
        }

        private void ViewModel_MessageBoxRequest(object sender, MessageBoxEventArgs e)
        {
            e.Show(this);
        }

        private void ViewModel_FileDialogRequest(object sender, FileDialogEventArgs e)
        {
            e.ShowDialog(this);
        }
    }
}
