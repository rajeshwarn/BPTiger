using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Profile;
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
    internal class RestoreBackupICommand<T> : ICommand
    {
        public RestoreBackupICommand()
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

            bool bExecute = (profile != null);

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
        }
    }
}
