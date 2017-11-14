using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Models.FolderSelection;
using CompleteBackup.ViewModels.FolderSelection.ICommands;
using CompleteBackup.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace CompleteBackup.ViewModels
{
    public class RestoreItemsSelectionViewModel : ObservableObject
    {
        public ICommand OpenSelectBackupItemsWindowCommand { get; private set; } = new OpenSelectBackupItemsWindowICommand<object>();
        public ICommand SelectFolderNameCommand { get; private set; } = new SelectRestoreDestinationFolderICommand<object>();


        public BackupProjectData ProjectData { get; set; } = BackupProjectRepository.Instance.SelectedBackupProject;

        public ObservableCollection<FolderData> SelectionFolderList { get; set; } = BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile?.RestoreFolderList;//= new ObservableCollection<FolderData>();

        //public void OpenSelectionWindow()
        //{
        //    new SelectRestoreItemsWindow().Show();
        //}


        public string SourceFileListGroupTitle { get; set; } = "Items To Restore";
        public string SourceFileActionTitle { get; set; } = "Select Items";
        public string DestinationGroupTitle { get; set; } = "Destination";

        private void ProfileDataUpdate(BackupProfileData profile)
        {
        }
        public RestoreItemsSelectionViewModel()
        {
            //Register to get update event when backup profile changed
            BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile?.ProfileDataRefreshTask?.RegisterEvent(ProfileDataUpdate);
        }
    }
}

