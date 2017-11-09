using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
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
            bool bExecute = true;
            return bExecute;
        }

        public void Execute(object parameter)
        {
            var paramList = parameter as IList<object>;

            var profile = paramList[0] as BackupProfileData;

            var project = BackupProjectRepository.Instance.SelectedBackupProject;

            //TODO check if backup is running
            project.BackupProfileList.Remove(profile);

            BackupProjectRepository.Instance.SaveProject();
        }
    }
}
