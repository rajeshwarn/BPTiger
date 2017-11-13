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
    internal class CancelPendingBackupTaskICommand<T> : ICommand
    {
        public CancelPendingBackupTaskICommand()
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
                if ((profile != null) && (BackupTaskManager.Instance.IsBackupWorkerPending(profile) == true))
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

            BackupTaskManager.Instance.CancelPendingBackupTask(profile);
        }
    }
}
