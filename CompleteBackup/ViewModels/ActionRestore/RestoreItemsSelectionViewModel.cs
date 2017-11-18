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
    public class RestoreItemsSelectionViewModel : GenericBackupItemsSelectionViewModel
    {
        public RestoreItemsSelectionViewModel()
        {
            TargetFolderGaugeList.Add(new ChartGaugeView(System.Windows.Media.Brushes.Red, System.Windows.Media.Brushes.Green, System.Windows.Media.Brushes.Yellow) { PumpNumber = null, GaugeValue = 0.0F });
        }

        public override bool Enabled { get { return ProjectData.CurrentBackupProfile.LastBackupDateTime != null; } }
        public override bool IsBackupView { get; } = false;

        public override ICommand OpenItemSelectWindowCommand { get; } = new OpenSelectRestoreItemsWindowICommand<object>();
        public override ICommand SelectTargetFolderNameCommand { get; } = new SelectTargetRestoreFolderNameICommand<object>();

        public override ObservableCollection<BackupPerfectAlertData> BackupAlertList { get { return ProjectData?.CurrentBackupProfile?.RestoreAlertList; } }
        public override ObservableCollection<FolderData> SelectionFolderList { get { return ProjectData?.CurrentBackupProfile?.RestoreFolderList; } }
        public override ObservableCollection<FolderData> DestinationFolderList { get { return ProjectData?.CurrentBackupProfile?.TargetRestoreFolderList; } }
        public override ObservableCollection<ChartGaugeView> TargetFolderGaugeList { get; } = new ObservableCollection<ChartGaugeView>();

        public override long? SelectionFolderListNumberOfFiles { get { return ProjectData.CurrentBackupProfile?.RestoreSourceFilesNumber; } }
        public override long? SelectionTotalFolderListSize { get { return ProjectData.CurrentBackupProfile?.RestoreSourceFoldersSize; } }


        public override string SourceFileListGroupTitle { get; } = "Items To Restore";
        public override string SourceFileActionTitle { get; } = "Select Items";
        public override string DestinationGroupTitle { get; } = "Destination";
    }
}

