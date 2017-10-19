using CompleteBackup.Models.Backup.Profile;
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
            var profileData = parameter as BackupSetData;
            if (profileData != null)
            {
                //var mongoDBSettings = parameters[0] as Properties.MongoDBSettings;
                //var jagSettins = parameters[0] as Properties.JagSettings;
                //var propName = parameters[1] as string;

                var fileDialog = new System.Windows.Forms.FolderBrowserDialog();

                switch (fileDialog.ShowDialog())
                {
                    case System.Windows.Forms.DialogResult.OK:

                        try
                        {
                            profileData.TargetBackupFolder = fileDialog.SelectedPath;

                            if (!profileData.IsValidSetData)
                            {
                                MessageBox.Show($"The destination folder you have selected does not contain a valid backup set", "Destination folder", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        catch (System.IO.PathTooLongException ex)
                        {
                            MessageBox.Show($"The destination path you have selected is too long, please select a shorter", "Destination folder", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        break;

                    case System.Windows.Forms.DialogResult.Cancel:
                    //Do nothiong
                    default:
                        break;
                }
            }
        }
    }
}
