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
    internal class SelectTargetRestoreFolderNameICommand<T> : ICommand
    {
        public SelectTargetRestoreFolderNameICommand() { }

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

        void SaveRestoreFolder(BackupProfileData profile, string path)
        {

            //TODO - only one destination folder is supported right now
            profile.TargetRestoreFolderList.Clear();
            var folderData = new FolderData() { IsFolder = true, IsAvailable = true, Path = path};
            profile.TargetRestoreFolderList.Add(folderData);

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
                                        MessageBox.Show($"The destination folder you have selected is not a valid directory", "Restore destination", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                    else
                                    {
                                        var folderStatus = profile.GetProfileTargetFolderStatus(path);
                                        switch (folderStatus)
                                        {
                                            case ProfileTargetFolderStatusEnum.AssosiatedWithThisProfile:
                                            case ProfileTargetFolderStatusEnum.AssosiatedWithADifferentProfile:
                                            case ProfileTargetFolderStatusEnum.CoccuptedOrNotRecognizedProfile:
                                                MessageBoxResult result = MessageBox.Show($"The restore directory you have selected contains backup item, Are you sure you want to use this directory to store your restore backup items?", "Restore destination", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                                                if (result == MessageBoxResult.Yes)
                                                {
                                                    SaveRestoreFolder(profile, fileDialog.SelectedPath);
                                                    bRetry = false;
                                                }
                                                break;

                                            case ProfileTargetFolderStatusEnum.EmptyFolderNoProfile:
                                                result = MessageBox.Show($"The restore directory you have selected is not empty, Are you sure you want to use this directory to store your restore backup items?", "Restore destination", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                                                if (result == MessageBoxResult.Yes)
                                                {
                                                    SaveRestoreFolder(profile, fileDialog.SelectedPath);
                                                    bRetry = false;
                                                }

                                                break;
                                            default:
                                                SaveRestoreFolder(profile, fileDialog.SelectedPath);
                                                bRetry = false;

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
                                bRetry = false;

                                break;
                        }
                    }
                }
            }
        }
    }
}
