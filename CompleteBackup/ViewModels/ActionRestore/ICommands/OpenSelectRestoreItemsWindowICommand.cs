using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.FolderSelection.ICommands
{
    internal class OpenSelectRestoreItemsWindowICommand<T> : ICommand
    {
        public OpenSelectRestoreItemsWindowICommand()
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

            var profile = parameter as BackupProfileData;
            if (profile != null)
            {
                bExecute = (profile.IsValidFolderName(profile.GetTargetBackupFolder()) && (profile.BackupFolderList.Count() > 0));
            }

            return bExecute;
        }

        public void Execute(object parameter)
        {
            new SelectRestoreItemsWindow().Show();
        }
    }
}
