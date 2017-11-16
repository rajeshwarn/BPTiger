using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Models.FolderSelection;
using CompleteBackup.ViewModels.FolderSelection.ICommands;
using CompleteBackup.ViewModels.FolderSelection.Validators;
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
    public class RestoreItemsSelectionViewModel : GenericBackupItemsSelectionViewModel
    {
        public override ICommand OpenItemSelectWindowCommand { get; } = new OpenSelectRestoreItemsWindowICommand<object>();
        public override ICommand SelectTargetFolderNameCommand { get; } = new SelectTargetRestoreFolderNameICommand<object>();

        public override ObservableCollection<BackupPerfectAlertData> BackupAlertList { get { return BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile?.RestoreAlertList; } }
        public override ObservableCollection<FolderData> SelectionFolderList { get { return BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile?.RestoreFolderList; } }
        public override string DestinationFolderName { get { return BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile?.TargetRestoreFolder; }
            set { if (BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile != null) { BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile.TargetRestoreFolder = value; OnPropertyChanged(); } } }
        public override long? SelectionFolderListNumberOfFiles { get { return ProjectData.CurrentBackupProfile?.RestoreSourceFilesNumber; } }
        public override long? SelectionTotalFolderListSize { get { return ProjectData.CurrentBackupProfile?.RestoreSourceFoldersSize; } }


        public override string SourceFileListGroupTitle { get; } = "Items To Restore";
        public override string SourceFileActionTitle { get; } = "Select Items";
        public override string DestinationGroupTitle { get; } = "Destination";
    }
}

