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
    internal class SelectFolderNameICommand<T> : ICommand
    {
        public SelectFolderNameICommand() { }

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
                                        MessageBox.Show($"The destination folder you have selected does not contain a valid backup set", "Destination folder", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                    else
                                    {
                                        var folderStatus = profile.GetProfileTargetFolderStatus(path);
                                        switch (folderStatus)
                                        {
                                            case BackupProfileData.ProfileTargetFolderStatusEnum.AssosiatedWithThisProfile:

                                                profile.TargetBackupFolder = fileDialog.SelectedPath;
                                                bRetry = false;

                                                break;

                                            case BackupProfileData.ProfileTargetFolderStatusEnum.AssosiatedWithADifferentProfile:
                                            case BackupProfileData.ProfileTargetFolderStatusEnum.CoccuptedOrNotRecognizedProfile:

                                                MessageBoxResult result = MessageBox.Show($"The backup folder is assosiated with a different Backup Profile or corrupted\n\nWould you like to try to convert and associate this target folder to this Backup Profile?\nPress Yes to try to convert or No if you are not sure", "Backup folder", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);

                                                if (result == MessageBoxResult.Yes)
                                                {
                                                    if (profile.ConverBackupProfileFolderToNewPath(path) > 0)
                                                    {
                                                        MessageBox.Show($"Backup profile Succesfully converted!", "Destination folder", MessageBoxButton.OK, MessageBoxImage.Information);
                                                        profile.TargetBackupFolder = fileDialog.SelectedPath;

                                                        bRetry = false;
                                                    }
                                                    else
                                                    {
                                                        MessageBox.Show($"Failed to convert Backup Folder", "Destination folder", MessageBoxButton.OK, MessageBoxImage.Error);
                                                    }
                                                }
                                                else
                                                {
                                                    bRetry = false;
                                                }

                                                break;
                                            default:
                                                {
                                                    MessageBox.Show($"folder error {folderStatus.ToString()}", "Destination folder", MessageBoxButton.OK, MessageBoxImage.Error);
                                                    profile.TargetBackupFolder = fileDialog.SelectedPath;
                                                    bRetry = false;
                                                }
                                                break;
                                        }
                                    }
                                }
                                catch (System.IO.PathTooLongException ex)
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
