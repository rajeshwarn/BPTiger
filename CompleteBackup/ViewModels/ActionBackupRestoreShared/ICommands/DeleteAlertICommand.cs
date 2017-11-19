using CompleteBackup.DataRepository;
using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Profile;
using CompleteBackup.Views;
using CompleteBackup.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.ICommands
{
    internal class DeleteAlertICommand<T> : ICommand
    {
        public DeleteAlertICommand()
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
            var alert = parameter as BackupPerfectAlertData;
            var profile = BackupProjectRepository.Instance?.SelectedBackupProject?.CurrentBackupProfile;

            return (alert != null) && alert.IsDeletable && (profile != null);
        }

        public void Execute(object parameter)
        {
            var alert = parameter as BackupPerfectAlertData;

            var profile = BackupProjectRepository.Instance?.SelectedBackupProject?.CurrentBackupProfile;

            BackupAlertManager.Instance.RemoveAlert(profile, alert.AlertType);            
        }
    }
}
