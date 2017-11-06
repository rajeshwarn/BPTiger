using CompleteBackup.DataRepository;
using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Views;
using CompleteBackup.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.ICommands
{
    internal class SelectBackupProfileICommand<T> : ICommand
    {
        public SelectBackupProfileICommand()
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

            bool bExecute = profile != null;

            if (!bExecute)
            {

            }

            return bExecute;
        }

        public void Execute(object parameter)
        {
            var profile = parameter as BackupProfileData;
            BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile = profile;
        }
    }
}
