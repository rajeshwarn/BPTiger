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
    internal class StartWatcherBackupICommand<T> : ICommand
    {
        public StartWatcherBackupICommand()
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

            bool bExecute = (profile != null) && (BackupTaskManager.Instance.IsBackupWorkerBusy(profile) == false) && (BackupTaskManager.Instance.IsBackupWorkerPaused(profile) == false);

            return bExecute;
        }

        public void Execute(object parameter)
        {
            var profile = parameter as BackupProfileData;

            BackupTaskManager.Instance.StartBackup(profile, false);
        }
    }
}
