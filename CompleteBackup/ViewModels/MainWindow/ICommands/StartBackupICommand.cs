using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Views;
using CompleteBackup.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.MainWindow.ICommands
{
    internal class StartBackupICommand<T> : ICommand
    {
        public StartBackupICommand()
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
            var profile = parameter as BackupProfileData;
//            var folderSelection = folderTree.DataContext as SourceBackupItemsTreeViewModel;

            var progressBar = GenericStatusBarView.NewInstance;
            progressBar.UpdateProgressBar("Backup starting...", 0);

            var backup = BackupFactory.CreateFullBackupTask(profile.FolderList.ToList<string>(), profile.TargetBackupFolder, progressBar);
            backup.RunWorkerAsync();
        }
    }
}
