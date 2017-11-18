using CompleteBackup.DataRepository;
using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Profile;
using CompleteBackup.ViewModels.MainWindow;
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
            var mainWindowVM = parameter as MainWindowViewModel;
            var pauseData = parameter as BackupPauseData;

            var profile = BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile;

            bool bExecute = ((pauseData != null && !profile.IsBackupSleep) || (mainWindowVM != null))&& (profile != null);

            return bExecute;
        }

        public void Execute(object parameter)
        {
            var pauseData = parameter as BackupPauseData;
            var profile = BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile;
            var mainWindowVM = parameter as MainWindowViewModel;

            if (profile.IsBackupWorkerBusy == true && !profile.IsBackupSleep)
            {
                MessageBoxResult result = MessageBox.Show($"Back up in progress, are you sure you want to sleep now and stop the backup process for this profile now, you can always start the backup again later?", "Backup sleep", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                profile.Logger.Writeln($"Entering Sleep mode, stopping running/pending taskas");
                BackupTaskManager.Instance.CancelPendingBackupTask(profile);
                BackupTaskManager.Instance.StopBackupTask(profile);
            }

            if (pauseData != null)
            {
                profile.Logger.Writeln($"Sleep mode set to {pauseData.Hours} hours");
                profile.SleepBackup(pauseData.Hours);
            }
            else
            {
                if (profile.IsBackupSleep)
                {
                    profile.SleepBackup(0);
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show($"Sleep for 1 hour (default value)?", "Backup sleep", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (result != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    profile.Logger.Writeln($"Sleep mode set to a default 1 hour");
                    profile.SleepBackup(1);
                }
            }
        }
    }
}
