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
    internal class SleepBackupProfileICommand<T> : ICommand
    {
        public SleepBackupProfileICommand()
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
            var pauseData = parameter as BackupPauseData;

            var profile = BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile;

            bool bExecute = (pauseData != null) && (profile != null);

            return bExecute;
        }

        public void Execute(object parameter)
        {
            var pauseData = parameter as BackupPauseData;
            var profile = BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile;

            if (profile.IsBackupWorkerBusy == true)
            {
                MessageBoxResult result = MessageBox.Show($"Back up in progress, are you sure you want to sleep now and pause the backup process for this profile now?", "Backup sleep", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                //TODO
                //Pause backup
            }

            profile.SleepBackup(pauseData.Hours);
        }
    }
}
