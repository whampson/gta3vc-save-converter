using System;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Input;
using GTASaveData;
using GTASaveData.GTA3;
using GTASaveData.VC;
using SaveConverter.Extensions;
using WpfEssentials;
using WpfEssentials.Win32;

namespace SaveConverter
{
    public class SaveFileConverter : ObservableObject
    {
        public static FileFormat[] GTA3Formats => new []
        {
            SaveFileGTA3.FileFormats.Android,
            SaveFileGTA3.FileFormats.iOS
        };

        public static FileFormat[] GTAVCFormats => new[]
        {
            SaveFileVC.FileFormats.Android,
            SaveFileVC.FileFormats.iOS
        };

        public event EventHandler<MessageBoxEventArgs> MessageBoxRequest;
        public event EventHandler<FileDialogEventArgs> FileDialogRequest;

        private SaveFileGTA3VC m_save;
        private FileFormat m_newFormat;
        private FileFormat m_format;
        private string m_lastDirectoryAccessed;
        private string m_gameName;
        private string m_name;
        private string m_safehouse;
        private float m_progress;

        public bool IsGTA3 => TheSave is SaveFileGTA3;
        public bool IsViceCity => TheSave is SaveFileVC;

        public SaveFileGTA3VC TheSave
        {
            get { return m_save; }
            set { m_save = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGTA3));
                OnPropertyChanged(nameof(IsViceCity));
            }
        }

        public string LastDirectoryAccessed
        {
            get { return m_lastDirectoryAccessed; }
            set { m_lastDirectoryAccessed = value; OnPropertyChanged(); }
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

        public string GameNameOnDisplay
        {
            get { return m_gameName; }
            set { m_gameName = value; OnPropertyChanged(); }
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

        private void Update()
        {
            UpdateGameName();
            UpdateName();
            UpdateFormatOnDisplay();
            UpdateProgress();
        }

        private void UpdateGameName()
        {
            GameNameOnDisplay =
                (IsGTA3) ? "Grand Theft Auto III" :
                (IsViceCity) ? "Grand Theft Auto: Vice City" :
                "";
        }

        private void UpdateName()
        {
            SaveNameOnDisplay = TheSave?.GetSaveName() ?? "";
        }

        public void UpdateFormatOnDisplay()
        {
            SaveFormatOnDisplay = TheSave?.FileFormat ?? FileFormat.Default;
        }

        private void UpdateProgress()
        {
            ProgressOnDisplay = TheSave?.GetProgress() ?? 0;
        }

        public void OpenFile(string path)
        {
            try
            {
                if (SaveFile.TryLoad(path, out SaveFileGTA3 gta3save))
                {
                    TheSave = gta3save;
                    if (TheSave.FileFormat.IsAndroid)
                    {
                        SelectedFormat = SaveFileGTA3.FileFormats.iOS;
                    }
                    else if (TheSave.FileFormat.IsiOS)
                    {
                        SelectedFormat = SaveFileGTA3.FileFormats.Android;
                    }
                }
                else if (SaveFile.TryLoad(path, out SaveFileVC vcsave))
                {
                    TheSave = vcsave;
                    if (TheSave.FileFormat.IsAndroid)
                    {
                        SelectedFormat = SaveFileVC.FileFormats.iOS;
                    }
                    else if (TheSave.FileFormat.IsiOS)
                    {
                        SelectedFormat = SaveFileVC.FileFormats.Android;
                    }
                }
            }
            catch (Exception ex)
            {
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
                DebugHelper.Throw(ex);
            }
            finally
            {
                Update();
            }
        }

        public bool SaveCurrentFile(string path)
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

                DebugHelper.Throw(ex);
                return false;
            }
        }

        public void CloseFile()
        {
            if (TheSave != null)
            {
                TheSave.Dispose();
            }

            Update();
        }

        public ICommand Open => new RelayCommand
        (
            () => ShowFileDialog(FileDialogType.OpenFileDialog, (r, e) =>
            {
                LastDirectoryAccessed = Path.GetDirectoryName(e.FileName);
                if (r != true) return;

                CloseFile();
                OpenFile(e.FileName);
                if (TheSave == null)
                {
                    ShowError("The file is not a valid GTA3 or Vice City save file.", "Invalid Save File");
                }
                else if (!TheSave.FileFormat.IsMobile)
                {
                    ShowError("Please select an Android or iOS save.", "Invalid Save Format");
                    TheSave.Dispose();
                    TheSave = null;
                }
            })
        );

        public ICommand Convert => new RelayCommand
        (
            () => ShowFileDialog(FileDialogType.SaveFileDialog, (r, e) =>
            {
                LastDirectoryAccessed = Path.GetDirectoryName(e.FileName);
                if (r != true) return;
                
                TheSave.FileFormat = SelectedFormat;
                bool success = SaveCurrentFile(e.FileName);
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
                $"This tool allows you to convert your GTA Android saves to iOS and\n" +
                $"vice-versa. Supports Grand Theft Auto 3 and Vice City saves.\n" +
                $"\n" +
                $"Thanks to Lethal Vaccine for the idea!\n" +
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
                InitialDirectory = LastDirectoryAccessed ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "GTA3/VC Save Files|*.b|All Files|*.*",
            };
            FileDialogRequest?.Invoke(this, e);
        }
    }
}
