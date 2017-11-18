using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.FolderSelection.ICommands
{
    internal class OpenItemLocationICommand<T> : ICommand
    {
        public OpenItemLocationICommand() { }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            var paramList = parameter as IList<object>;

            return (paramList != null) && (paramList.Count() > 0);
        }

        public void Execute(object parameter)
        {
            var paramList = parameter as IList<object>;
            var folderData = paramList[0] as FolderData;
            var profile = BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile;

            var path = folderData.Path;
            if (!folderData.IsFolder)
            {
                path = profile.GetStorageInterface().GetDirectoryName(folderData.Path);
            }
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = path,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Could not open {path}\n{ex.Message}", "Open folder location", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
