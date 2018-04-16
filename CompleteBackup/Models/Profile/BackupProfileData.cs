using CompleteBackup.DataRepository;
using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup.History;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Models.FolderSelection;
using CompleteBackup.Models.Profile;
using CompleteBackup.Models.Utilities;
using CompleteBackup.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Xml.Serialization;

namespace CompleteBackup.Models.Backup.Profile
{
    public class BackupProfileData : ObservableObject
    {
        [XmlIgnore]
        public static string TargetBackupBaseDirectoryName { get; } = "Archive";
        
        //General properties
        private BackupTypeEnum m_BackupType { get; set; } = BackupTypeEnum.Snapshot;
        public BackupTypeEnum BackupType { get { return m_BackupType; } set { m_BackupType = value; OnPropertyChanged(); OnPropertyChanged("BackupTypeName"); OnPropertyChanged("BackupTypeImage"); } }

        [XmlIgnore]
        public bool IsDifferentialBackup { get { return BackupType == BackupTypeEnum.Differential; } }

        public Guid GUID { get; set; } = Guid.NewGuid();

        private string m_Name;
        public string Name { get { return m_Name; } set { m_Name = value; OnPropertyChanged(); } }

        private string m_Description;
        public string Description { get { return m_Description; } set { m_Description = value; OnPropertyChanged(); } }

        //Properties collections
        public ObservableCollection<FolderData> BackupFolderList { get; set; } = new ObservableCollection<FolderData>();
        public ObservableCollection<FolderData> TargetBackupFolderList { get; set; } = new ObservableCollection<FolderData>();

        [XmlIgnore]
        public ObservableCollection<FolderData> RestoreFolderList { get; set; } = new ObservableCollection<FolderData>();
        public ObservableCollection<FolderData> TargetRestoreFolderList { get; set; } = new ObservableCollection<FolderData>();

        public ObservableCollection<FileSystemWatcherItemData> BackupWatcherItemList { get; set; } = new ObservableCollection<FileSystemWatcherItemData>();



        //Profile - Helpers
        [XmlIgnore]
        public bool IsDetaledLog { get; set; } = false;

        [XmlIgnore]
        public bool IsCurrentProfileSelected { get { return BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile == this; } set { OnPropertyChanged(); } }

        [XmlIgnore]
        public string BackupSignature { get { return $"{ GUID.ToString("D")}-BC-{BackupType}"; } }


        private DateTime? m_LastBackupDateTime { get; set; }
        public DateTime? LastBackupDateTime { get { return m_LastBackupDateTime; } set { m_LastBackupDateTime = value; OnPropertyChanged(); } }

        [XmlIgnore]
        public string BackupTypeName { get { return ProfileHelper.BackupTypeList.FirstOrDefault(i => i.BackupType == m_BackupType)?.Name; } set { } }
        [XmlIgnore]
        public string BackupTypeImage { get { return ProfileHelper.BackupTypeList.FirstOrDefault(i => i.BackupType == m_BackupType)?.ImageName; } set { } }


        private bool m_IsWatchFileSystem { get; set; } = true;
        public bool IsWatchFileSystem { get { return m_IsWatchFileSystem; } set {
                if (m_IsWatchFileSystem != value)
                {
                    if (value)
                    {
                        FileSystemWatcherWorker?.UpdateFileSystemWatcherInterval(m_UpdateWatchItemsTimeSeconds * 1000);
                    }
                    else
                    {
                        FileSystemWatcherWorker?.UpdateFileSystemWatcherInterval(0);
                    }
                }

                m_IsWatchFileSystem = value;
                OnPropertyChanged();
                BackupProjectRepository.Instance?.SaveProject();
            }
        }

        [XmlIgnore]
        public bool IsBackupSleep { get { return DateTime.Compare(WateUpFromSleepTime, DateTime.Now) > 0; } set { OnPropertyChanged(); }
        }
        public DateTime WateUpFromSleepTime { get; set; }

        //Time to backup new changes in seconds
        private long m_UpdateWatchItemsTimeSeconds = 6;
        public long UpdateWatchItemsTimeSeconds { get { return m_UpdateWatchItemsTimeSeconds; } set { m_UpdateWatchItemsTimeSeconds = value; FileSystemWatcherWorker?.UpdateFileSystemWatcherInterval(value * 1000); OnPropertyChanged(); BackupProjectRepository.Instance?.SaveProject(); } }



        //Alerts
        [XmlIgnore]
        public ObservableCollection<BackupPerfectAlertData> BackupAlertList = new ObservableCollection<BackupPerfectAlertData>();

        [XmlIgnore]
        public ObservableCollection<BackupPerfectAlertData> RestoreAlertList = new ObservableCollection<BackupPerfectAlertData>();


        //Helpers
        [XmlIgnore]
        public bool FileSystemWatcherEnabled { get { return BackupType != BackupTypeEnum.Snapshot; } }

        [XmlIgnore]
        public FileSystemWatcherWorkerTask FileSystemWatcherWorker { get; set; }

        [XmlIgnore]
        public ProfileDataRefreshWorkerTask ProfileDataRefreshTask { get; set; }

        [XmlIgnore]
        public BackupPerfectLogger Logger { get; } = new BackupPerfectLogger();

        private IStorageInterface m_IStorage = new FileSystemStorage();
        public IStorageInterface GetStorageInterface() { return m_IStorage; }



        [XmlIgnore]
        public bool IsValidProfileFolder { get { return this.GetProfileTargetFolderStatus(this.GetTargetBackupFolder()) != ProfileTargetFolderStatusEnum.AssosiatedWithThisProfile; } }

        [XmlIgnore]
        public ProfileTargetFolderStatusEnum ProfileTargetFolderStatus { get { return this.GetProfileTargetFolderStatus(this.GetTargetBackupFolder()); } }

        [XmlIgnore]
        public bool? IsBackupWorkerBusy { get { return BackupTaskManager.Instance.IsBackupWorkerBusy(this); } set { OnPropertyChanged(); OnPropertyChanged("IsBackupWorkerPending"); } }

        [XmlIgnore]
        public bool? IsBackupWorkerPending { get { return BackupTaskManager.Instance.IsBackupWorkerPending(this); } set { OnPropertyChanged(); } }

        [XmlIgnore]
        public bool? IsBackupWorkerPaused { get { return BackupTaskManager.Instance.IsBackupWorkerPaused(this); } set { OnPropertyChanged(); } }


        //File system properties
        public long BackupTargetDiskSizeNumber { get; set; } = 0;

        public long BackupTargetUsedSizeNumber { get; set; } = 0;

        public long BackupTargetFreeSizeNumber { get; set; } = 0;

        public long m_BackupSourceFilesNumber { get; set; } = 0;
        public long BackupSourceFilesNumber { get { return m_BackupSourceFilesNumber; } set { m_BackupSourceFilesNumber = value; OnPropertyChanged(); } }

        public long m_BackupSourceFoldersSize { get; set; } = 0;
        public long BackupSourceFoldersSize { get { return m_BackupSourceFoldersSize; } set { m_BackupSourceFoldersSize = value; OnPropertyChanged(); } }


        public long m_RestoreSourceFilesNumber { get; set; } = 0;
        public long RestoreSourceFilesNumber { get { return m_RestoreSourceFilesNumber; } set { m_RestoreSourceFilesNumber = value; OnPropertyChanged(); } }

        public long m_RestoreSourceFoldersSize { get; set; } = 0;
        public long RestoreSourceFoldersSize { get { return m_RestoreSourceFoldersSize; } set { m_RestoreSourceFoldersSize = value; OnPropertyChanged(); } }
    }
}
