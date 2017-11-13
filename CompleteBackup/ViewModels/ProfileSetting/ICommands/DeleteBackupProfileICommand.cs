using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Profile;
using CompleteBackup.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.ICommands
{
    internal class DeleteBackupProfileICommand<T> : ICommand
    {
        public DeleteBackupProfileICommand()
        {
        }
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
            bool bExecute = false;

            var paramList = parameter as IList<object>;
            if (paramList != null && paramList.Count() > 0)
            {
                var profile = paramList[0] as BackupProfileData;
                if ((profile != null) && (BackupTaskManager.Instance.IsBackupWorkerBusy(profile) != true))
                {
                    bExecute = true;
                }
            }

            return bExecute;
        }

        public void Execute(object parameter)
        {
            var paramList = parameter as IList<object>;
            var profile = paramList[0] as BackupProfileData;

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete '{profile.Name}' backup profile?\nOnly the backup profile will be deleted from this application.  All your files on your storage device/s will not be deleted.", "Delete Profile", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool? bBusy = false;
                lock (BackupTaskManager.Instance)
                {
                    var project = BackupProjectRepository.Instance.SelectedBackupProject;
                    if (project != null)
                    {
                        bBusy = BackupTaskManager.Instance.IsBackupWorkerBusy(profile);
                        if (bBusy != true)
                        {
                            project.BackupProfileList.Remove(profile);
                            project.CurrentBackupProfile = null;
                        }
                    }
                }

                if (bBusy == true)
                {
                    MessageBox.Show($"Backup profile '{profile.Name}' in use.  If backup is running on this profile, wait until it completed", "Delete Profile", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
                else
                {
                    BackupProjectRepository.Instance.SaveProject();
                }
            }
        }
    }
}
