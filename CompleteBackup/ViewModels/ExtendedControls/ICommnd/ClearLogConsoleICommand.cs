using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using CompleteBackup.Models.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.ICommands
{
    internal class ClearLogConsoleICommand<T> : ICommand
    {
        public ClearLogConsoleICommand() { }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                //                CommandManager.RequerySuggested += value;
            }
            remove
            {
                //                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }


        public void Execute(object parameter)
        {
            var profile = parameter as BackupProfileData;
            profile.Logger.Clear();
        }
    }
}
