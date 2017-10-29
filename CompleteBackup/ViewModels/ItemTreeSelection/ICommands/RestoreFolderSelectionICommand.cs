using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.FolderSelection.ICommands
{
    internal class RestoreFolderSelectionICommand<T> : ICommand
    {
        public RestoreFolderSelectionICommand() { }

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
            bool bExecute = false;
            var viewModel = parameter as SelectRestoreItemsWindowModel;
        
            if (viewModel?.DirtyFlag == true)
            {
                bExecute = true;
            }

            return bExecute;
        }

        public void Execute(object parameter)
        {
            var viewModel = parameter as SelectRestoreItemsWindowModel;

            if (viewModel != null)
            {
                viewModel.DirtyFlag = false;
            }

            viewModel.UpdateSelectedFolderList();

            viewModel.ProjectData?.CurrentBackupProfile.RestoreFolderList.Clear();
            foreach (var item in viewModel.SelectedItemList)
            {
                viewModel.ProjectData?.CurrentBackupProfile.RestoreFolderList.Add(item);
            }

            //viewModel.ProjectData?.CurrentBackupProfile.UpdateProfileProperties();

            //BackupProjectRepository.Instance.SaveProject();
        }
    }
}
