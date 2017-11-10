using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.FolderSelection.ICommands
{
    internal class SelectRestoreDestinationFolderICommand<T> : ICommand
    {
        public SelectRestoreDestinationFolderICommand() { }

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

        void SaveDestinationFolder(BackupProfileData profile, string path)
        {
            profile.TargetRestoreFolder = path;

            BackupProjectRepository.Instance.SaveProject();
        }

        public void Execute(object parameter)
        {
            var profile = parameter as BackupProfileData;
            if (profile != null)
            {
                using (var fileDialog = new System.Windows.Forms.FolderBrowserDialog() { ShowNewFolderButton = true })
                {
                    bool bRetry = true;
                    while (bRetry)
                    {
                        switch (fileDialog.ShowDialog())
                        {
                            case System.Windows.Forms.DialogResult.OK:

                                try
                                {
                                    var path = fileDialog.SelectedPath;

                                    if ((path == null) || (path == String.Empty))
                                    {
                                        MessageBox.Show($"The destination folder you have selected does is invalid", "Destination folder", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                    else
                                    {
                                        var folderStatus = profile.GetProfileTargetFolderStatus(path);
                                        switch (folderStatus)
                                        {
                                            case BackupProfileData.ProfileTargetFolderStatusEnum.EmptyFolderNoProfile:
                                                SaveDestinationFolder(profile, path);
                                                bRetry = false;

                                                break;

                                            default:
                                                MessageBoxResult result = MessageBox.Show($"The Destination restore folder is not empty, do you want to use this selection?\n\nPress Yes to use this folder or No to select a different folder", "Restore folder", MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk);

                                                if (result == MessageBoxResult.Yes)
                                                {
                                                    SaveDestinationFolder(profile, path);
                                                    bRetry = false;
                                                }

                                                break;
                                        }
                                    }
                                }
                                catch (System.IO.PathTooLongException)
                                {
                                    MessageBox.Show($"The destination path you have selected is too long, please select a shorter", "Destination folder", MessageBoxButton.OK, MessageBoxImage.Error);
                                }

                                break;

                            case System.Windows.Forms.DialogResult.Cancel:
                                bRetry = false;

                                break;

                            default:

                                break;
                        }                        
                    }
                }
            }
        }
    }
}
