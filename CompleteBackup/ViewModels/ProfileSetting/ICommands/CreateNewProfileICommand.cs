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
    internal class CreateNewProfileICommand<T> : ICommand
    {
        public CreateNewProfileICommand()
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
            var window = parameter as Window;
            var vm = window.DataContext as CreateBackupProfileWindowViewModel;

            var profileType = vm.BackupTypeList.FirstOrDefault(b => b.IsChecked);
            vm.Profile.BackupType = profileType.BackupType;
            vm.Profile.InitProfile();

            vm.Project.BackupProfileList.Add(vm.Profile);

            BackupProjectRepository.Instance.SaveProject();

            window.Close();
        }
    }
}
