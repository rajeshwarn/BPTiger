using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.FolderSelection.ICommands
{
    internal class LocateMissingItemICommand<T> : ICommand
    {
        public LocateMissingItemICommand() { }

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
            var paramList = parameter as IList<object>;

            return (paramList != null) && (paramList.Count() > 0);
        }

        public void Execute(object parameter)
        {
            var paramList = parameter as IList<object>;
            var folderData = paramList[0] as FolderData;

            using (var fileDialog = new System.Windows.Forms.FolderBrowserDialog() { ShowNewFolderButton = false })
            {
                switch (fileDialog.ShowDialog())
                {
                    case System.Windows.Forms.DialogResult.OK:
                        {
                            var profile = BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile;
                            folderData.Path = fileDialog.SelectedPath;
                            folderData.Name = profile.GetStorageInterface().GetFileName(fileDialog.SelectedPath);
                            folderData.IsAvailable = true;

                            profile.UpdateProfileProperties();

                            //BackupProjectRepository.Instance.SaveProject();
                        }
                        break;
                
                    default:
                        break;
                }
            }
        }
    }
}
