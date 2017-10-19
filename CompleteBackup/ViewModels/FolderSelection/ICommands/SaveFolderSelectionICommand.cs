using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.FolderSelection.ICommands
{
    internal class SaveFolderSelectionICommand<T> : ICommand
    {
        public SaveFolderSelectionICommand() { }

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
            return true;
        }

        public void Execute(object parameter)
        {
            BackupProfileRepository.Instance.SaveProfile();
        }
    }
}
