using System;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Input;
using GTASaveData;
using GTASaveData.GTA3;
using WpfEssentials;
using WpfEssentials.Win32;

namespace SaveConverter
{
    public class SaveFileConverter : ObservableObject
    {
        public static readonly FileFormat[] ValidFormats = new FileFormat[]
        {
            GTA3Save.FileFormats.Android,
            GTA3Save.FileFormats.iOS
        };

        public event EventHandler<MessageBoxEventArgs> MessageBoxRequest;
        public event EventHandler<FileDialogEventArgs> FileDialogRequest;

        private GTA3Save m_save;
        private FileFormat m_newFormat;
        private FileFormat m_format;
        private string m_name;
        private string m_safehouse;
        private float m_progress;
        
        public GTA3Save TheSave
        {
            get { return m_save; }
            set { m_save = value; OnPropertyChanged(); }
        }

        public FileFormat SelectedFormat
        {
            get { return m_newFormat; }
            set { m_newFormat = value; OnPropertyChanged(); }
        }

        public FileFormat SaveFormatOnDisplay
        {
            get { return m_format; }
            set { m_format = value; OnPropertyChanged(); }
        }

        public string SaveNameOnDisplay
        {
            get { return m_name; }
            set { m_name = value; OnPropertyChanged(); }
        }

        public string SafeHouseOnDisplay
        {
            get { return m_safehouse; }
            set { m_safehouse = value; OnPropertyChanged(); }
        }

        public float ProgressOnDisplay
        {
            get { return m_progress; }
            set { m_progress = value; OnPropertyChanged(); }
        }

        private void UpdateName()
        {
            SaveNameOnDisplay = "";

            if (TheSave != null)
            {
                string name = TheSave.SimpleVars.LastMissionPassedName;
                if (string.IsNullOrEmpty(name))
                {
                    SaveNameOnDisplay = "(empty)";
                }
                else if (name[0] == '\xFFFF')
                {
                    if (!Gxt.TheText.TryGetValue(name.Substring(1), out string gxtName))
                    {
                        SaveNameOnDisplay = "(invalid GXT key)";
                    }
                    SaveNameOnDisplay = gxtName;
                }
                else
                {
                    SaveNameOnDisplay = name;
                }
            }
        }

        public void UpdateFormatOnDisplay()
        {
            SaveFormatOnDisplay = FileFormat.Default;

            if (TheSave != null)
            {
                SaveFormatOnDisplay = TheSave.FileFormat;
            }
        }

        private void UpdateSafeHouse()
        {
            SafeHouseOnDisplay = "";

            if (TheSave != null)
            {
                switch (TheSave.SimpleVars.CurrentLevel)
                {
                    case Level.Industrial: SafeHouseOnDisplay = "Portland"; break;
                    case Level.Commercial: SafeHouseOnDisplay = "Staunton Island"; break;
                    case Level.Suburban: SafeHouseOnDisplay = "Shoreside Vale"; break;
                }
            }
        }

        private void UpdateProgress()
        {
            ProgressOnDisplay = 0;

            if (TheSave != null)
            {
                ProgressOnDisplay = (float) TheSave.Stats.ProgressMade / TheSave.Stats.TotalProgressInGame;
            }
        }

        public void OpenFile(string path)
        {
            try
            {
                TheSave = GTA3Save.Load(path);

                if (TheSave.FileFormat.IsAndroid)
                {
                    SelectedFormat = GTA3Save.FileFormats.iOS;
                }
                else if (TheSave.FileFormat.IsiOS)
                {
                    SelectedFormat = GTA3Save.FileFormats.Android;
                }

                UpdateName();
                UpdateFormatOnDisplay();
                UpdateProgress();
                UpdateSafeHouse();

            }
            catch (Exception ex)
            {
                if (ex is InvalidDataException)
                {
                    ShowError("The file is not a valid GTA3 save file.");
                    return;
                }
                if (ex is FileNotFoundException || ex is DirectoryNotFoundException)
                {
                    ShowException(ex, "The path is invalid.");
                    return;
                }
                if (ex is IOException || ex is SecurityException)
                {
                    ShowException(ex, "An I/O error occurred.");
                    return;
                }

                ShowException(ex, "The file could not be opened.");

#if !DEBUG
                return;
#else
                throw;
#endif

            }
        }

        public bool SaveFile(string path)
        {
            try
            {
                TheSave.Save(path);
                return true;
            }
            catch (Exception ex)
            {
                if (ex is DirectoryNotFoundException)
                {
                    ShowException(ex, "The path is invalid.");
                }
                else if (ex is IOException || ex is SecurityException)
                {
                    ShowException(ex, "An I/O error occurred.");
                }
                else
                {
                    ShowException(ex, "The file could not be saved.");
                }

#if !DEBUG
                return false;
#else
                throw;
#endif
            }
        }

        public void CloseFile()
        {
            if (TheSave != null)
            {
                TheSave.Dispose();
            }

            UpdateName();
            UpdateFormatOnDisplay();
            UpdateSafeHouse();
            UpdateProgress();
        }

        public ICommand Open => new RelayCommand
        (
            () => ShowFileDialog(FileDialogType.OpenFileDialog, (r, e) =>
            {
                if (r != true) return;

                CloseFile();
                OpenFile(e.FileName);
                if (TheSave == null)
                {
                    ShowError("The file is not a valid GTA3 save file.");
                }
                else if (!TheSave.FileFormat.IsMobile)
                {
                    ShowError("The save format is invalid. Please select an Android or iOS save.");
                    TheSave.Dispose();
                    TheSave = null;
                }
            })
        );

        public ICommand Convert => new RelayCommand
        (
            () => ShowFileDialog(FileDialogType.SaveFileDialog, (r, e) =>
            {
                if (r != true) return;
                
                TheSave.FileFormat = SelectedFormat;
                bool success = SaveFile(e.FileName);
                if (success) ShowInfo("Conversion successful.", "Success");
            })
        );

        public ICommand Exit => new RelayCommand
        (
            () => Application.Current.MainWindow.Close()
        );

        public ICommand About => new RelayCommand
        (
            () => ShowInfo(
                $"{App.Name}\n" +
                $"Version: {App.Version}\n" +
                $"\n" +
                $"This tool allows you to convert your Android saves to iOS and\n" +
                $"your iOS saves to Android. Thanks to Lethal Vaccine for the idea!\n" +
                $"\n" +
                $"\n" +
                $"{App.Copyright}",
                title: "About")
        );

        public void ShowInfo(string text, string title = "Information")
        {
            MessageBoxRequest?.Invoke(this, new MessageBoxEventArgs(
                text, title, icon: MessageBoxImage.Information));
        }

        public void ShowWarning(string text, string title = "Warning")
        {
            MessageBoxRequest?.Invoke(this, new MessageBoxEventArgs(
                text, title, icon: MessageBoxImage.Warning));
        }

        public void ShowError(string text, string title = "Error")
        {
            MessageBoxRequest?.Invoke(this, new MessageBoxEventArgs(
                text, title, icon: MessageBoxImage.Error));
        }

        public void ShowException(Exception e, string text = "An error has occurred.", string title = "Error")
        {
            text += $"\n\n{e.GetType().Name}: {e.Message}";
            ShowError(text, title);
        }

        public void ShowFileDialog(FileDialogType type, Action<bool?, FileDialogEventArgs> callback)
        {
            FileDialogEventArgs e = new FileDialogEventArgs(type, callback)
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter = "GTA3 Save Files|*.b",
            };
            FileDialogRequest?.Invoke(this, e);
        }
    }
}
