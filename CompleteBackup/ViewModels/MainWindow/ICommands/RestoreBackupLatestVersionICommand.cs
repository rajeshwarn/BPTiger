using CompleteBackup.DataRepository;
using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Profile;
using CompleteBackup.ViewModels.FolderSelection.ICommands;
using CompleteBackup.Views;
using CompleteBackup.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.ICommands
{
    internal class RestoreBackupLatestVersionICommand<T> : ICommand
    {
        public RestoreBackupLatestVersionICommand()
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
            var profile = parameter as BackupProfileData;
            var currProfile = BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile;

            bool bExecute = currProfile != null;

            return bExecute;
        }

        public void Execute(object parameter)
        {
            var profile = parameter as BackupProfileData;

            //            new SelectRestoreItemsWindow().Show();


            if ((BackupTaskManager.Instance.IsBackupWorkerBusy(profile) == true) && (BackupTaskManager.Instance.IsBackupWorkerPaused(profile) == true))
            {
     //           profile.ResumeBackup();
            }
            else
            {
       //         profile.StartBackup();
            }

            var command = new OpenSelectRestoreItemsWindowICommand<object>();

            var currProfile = BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile;
            command.Execute(currProfile);
        }
    }
}
