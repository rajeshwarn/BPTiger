using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Models.FolderSelection;
using CompleteBackup.ViewModels.FolderSelection.ICommands;
using CompleteBackup.ViewModels.FolderSelection.Validators;
using CompleteBackup.Views;
using CompleteBackup.Views.ExtendedControls;
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
    public class BackupItemsSelectionViewModel : GenericBackupItemsSelectionViewModel
    {
        public BackupItemsSelectionViewModel()
        {
            float ratioTotal = 0;
            if (ProjectData.CurrentBackupProfile?.BackupTargetDiskSizeNumber != null && (float)ProjectData.CurrentBackupProfile?.BackupTargetDiskSizeNumber > 0)
            {
                ratioTotal = (float)ProjectData.CurrentBackupProfile?.BackupTargetFreeSizeNumber / (float)ProjectData.CurrentBackupProfile?.BackupTargetDiskSizeNumber;
            }
            TargetFolderGaugeList.Add(new ChartGaugeView(System.Windows.Media.Brushes.Red, System.Windows.Media.Brushes.Green, System.Windows.Media.Brushes.Yellow) { PumpNumber = "Storage", GaugeValue = ratioTotal });

            float ratiobackup = 0;
            if (ProjectData.CurrentBackupProfile?.BackupTargetDiskSizeNumber != null && (float)ProjectData.CurrentBackupProfile?.BackupTargetDiskSizeNumber > 0)
            {
                ratiobackup = (float)ProjectData.CurrentBackupProfile?.BackupTargetUsedSizeNumber / (float)ProjectData.CurrentBackupProfile?.BackupTargetDiskSizeNumber;
            }
            TargetFolderGaugeList.Add(new ChartGaugeView(System.Windows.Media.Brushes.Red, System.Windows.Media.Brushes.Green, System.Windows.Media.Brushes.Yellow) { PumpNumber = "Backup", GaugeValue = ratiobackup });
        }

        public override bool Enabled { get; } = true;
        public override bool IsBackupView { get; } = true;

        public override ICommand OpenItemSelectWindowCommand { get; } = new OpenSelectBackupItemsWindowICommand<object>();
        public override ICommand SelectTargetFolderNameCommand { get; } = new SelectTargetBackupFolderNameICommand<object>();

        public override ObservableCollection<BackupPerfectAlertData> BackupAlertList { get { return BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile?.BackupAlertList; } }

        public override ObservableCollection<FolderData> SelectionFolderList { get { return BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile?.BackupFolderList; } }
        public override ObservableCollection<FolderData> DestinationFolderList { get { return ProjectData?.CurrentBackupProfile?.TargetBackupFolderList; } }
        public override ObservableCollection<ChartGaugeView> TargetFolderGaugeList { get; } = new ObservableCollection<ChartGaugeView>();


        public override long? SelectionFolderListNumberOfFiles { get { return ProjectData.CurrentBackupProfile?.BackupSourceFilesNumber; } }
        public override long? SelectionTotalFolderListSize { get { return ProjectData.CurrentBackupProfile?.BackupSourceFoldersSize; } }


        public override string SourceFileListGroupTitle { get; } = "Items to Backup";
        public override string SourceFileActionTitle { get; } = "Change";
        public override string DestinationGroupTitle { get; } = "Destination";
    }
}

