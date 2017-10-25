using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Models.FolderSelection;
using CompleteBackup.ViewModels.FolderSelection.ICommands;
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
    public class SourceBackupItemsTreeViewModel : ObservableObject
    {
        public ICommand OpenChangeBackupItemsWindowCommand { get; private set; } = new OpenChangeBackupItemsWindowICommand<object>();
        public ICommand SelectFolderNameCommand { get; private set; } = new SelectFolderNameICommand<object>();


        public BackupProjectData ProjectData { get; set; } = BackupProjectRepository.Instance.SelectedBackupProject;


        private void ProfileDataUpdate(BackupProfileData profile)
        {
        }
        public SourceBackupItemsTreeViewModel()
        {
            //Register to get update event when backup profile changed
            BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile.RegisterEvent(ProfileDataUpdate);
        }
    }
}

