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
    internal class SelectBackupTypeICommand<T> : ICommand
    {
        public SelectBackupTypeICommand()
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

            bool bExecute = true;// (profile != null) && profile.IsBackupWorkerBusy && !profile.IsBackupWorkerPaused;

            return bExecute;
        }

        public void Execute(object parameter)
        {
            var vm = parameter as SelectBackupTypeWindowViewModel;

            var item = vm.BackupTypeList.FirstOrDefault(i => i.IsChecked);

            if (item.BackupType != vm.Project.SelectedBackupProject.CurrentBackupProfile.BackupType)
            {
                MessageBoxResult result = MessageBoxResult.None;
                result = MessageBox.Show($"Are you sure you want to change the current profile back up type to {item.Name}?", "Backup Type change", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    vm.Project.SelectedBackupProject.CurrentBackupProfile.BackupType = item.BackupType;
                    BackupProjectRepository.Instance.SaveProject();
                }
            }
            //            BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile = vm;

            //            profile.StartBackup();
        }
    }
}
