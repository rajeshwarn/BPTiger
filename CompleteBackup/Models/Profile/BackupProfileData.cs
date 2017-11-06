using CompleteBackup.DataRepository;
using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup.History;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Models.FolderSelection;
using CompleteBackup.Models.Profile;
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
using System.Windows;
using System.Xml.Serialization;

namespace CompleteBackup.Models.Backup.Profile
{
    public enum BackupRunTypeEnum
    {
        Always,
        Manual,
    }
    public enum BackupTypeEnum
    {
        Full,
        Incremental,
        Differential,
    }

    public class BackupTypeData
    {
        public BackupTypeEnum BackupType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageName { get; set; }
        public bool IsChecked { get; set; } = false;
    }

    class ProfileHelper
    {
        public static List<BackupTypeData> BackupTypeList { get; private set; } = new List<BackupTypeData>() {
            new BackupTypeData() { BackupType = BackupTypeEnum.Full, Name = "Full Backup", ImageName = @"/Resources/Icons/Ribbon/FullBackup.ico", Description = "A full copy of your entire data set"},
            new BackupTypeData() { BackupType = BackupTypeEnum.Incremental, Name = "Incremental Backup", ImageName = @"/Resources/Icons/Ribbon/IncrementalBackup.ico", Description = "Starts with a full backup, and subsequent backups only backup data that has changed\nFor a faster backup"},
            new BackupTypeData() { BackupType = BackupTypeEnum.Differential, Name = "Differential Backup", ImageName = @"/Resources/Icons/Ribbon/DifferentialBackup.ico", Description = "Similar to Incremental Backup, but also contains all the data that changed since the first backup\nThis lets you restore a specific version event if the data has been deleted in the source"},
        };
    }


    public class BackupProfileData : ObservableObject
    {
        public enum ProfileTargetFolderStatusEnum
        {
            AssosiatedWithThisProfile, //no error looks good
            AssosiatedWithADifferentProfile,
            CoccuptedOrNotRecognizedProfile,
            EmptyFolderNoProfile,
            InvalidTargetPath,
            NonEmptyFolderNoProfile,
        }

        public static Dictionary<ProfileTargetFolderStatusEnum, string> ProfileTargetFolderStatusDictionary { get; } = new Dictionary<ProfileTargetFolderStatusEnum, string>()
        {
            { ProfileTargetFolderStatusEnum.AssosiatedWithADifferentProfile, "The folder is associated with a different backup profile" },
            { ProfileTargetFolderStatusEnum.AssosiatedWithThisProfile, "" },
            { ProfileTargetFolderStatusEnum.CoccuptedOrNotRecognizedProfile, "The backup profile in this folder is not recognized or corrupted" },
            { ProfileTargetFolderStatusEnum.EmptyFolderNoProfile, "" },
            { ProfileTargetFolderStatusEnum.InvalidTargetPath, "The folder does not exist or invalid" },
            { ProfileTargetFolderStatusEnum.NonEmptyFolderNoProfile, "The folder does not contain a backup profile" },
        };


        public BackupProfileData()
        {
        }

        public void Init()
        {
            FileSystemWatcherWorker = new CBFileSystemWatcherWorker(this);

            if (!FileSystemWatcherWorker.IsBusy)
            {
                FileSystemWatcherWorker.RunWorkerAsync();
            }

            ProfileDataRefreshTask = new ProfileDataRefreshTask(this);
        }


        //Policy

        //Time to backup new changes in seconds
        public long BackupNewChangesTimeInterval { get; set; } = 60;


        [XmlIgnore]
        public CBFileSystemWatcherWorker FileSystemWatcherWorker { get; private set; }


        private string m_CurrentBackupFile;
        [XmlIgnore]
        public string CurrentBackupFile { get { return m_CurrentBackupFile; } set { m_CurrentBackupFile = value; OnPropertyChanged(); } }


        [XmlIgnore]
        public bool IsCurrent { get { return BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile == this; } set { OnPropertyChanged(); } }

        public string BackupSignature { get { return $"{GUID.ToString("D")}-BC-{BackupType}"; } }


        private DateTime? m_LastBackupDateTime { get; set; }
        public DateTime? LastBackupDateTime { get { return m_LastBackupDateTime; } set { m_LastBackupDateTime = value; OnPropertyChanged(); } }

        [XmlIgnore]
        public string BackupTypeName { get { return ProfileHelper.BackupTypeList.FirstOrDefault(i => i.BackupType == m_BackupType)?.Name; } set { } }
        [XmlIgnore]
        public string BackupTypeImage { get { return ProfileHelper.BackupTypeList.FirstOrDefault(i => i.BackupType == m_BackupType)?.ImageName; } set { } }


        
        private BackupRunTypeEnum m_BackupRunType { get; set; } = BackupRunTypeEnum.Always;
        public BackupRunTypeEnum BackupRunType { get { return m_BackupRunType; } set { m_BackupRunType = value; OnPropertyChanged(); } }

        private BackupTypeEnum m_BackupType { get; set; } = BackupTypeEnum.Full;
        public BackupTypeEnum BackupType { get { return m_BackupType; } set { m_BackupType = value; OnPropertyChanged(); OnPropertyChanged("BackupTypeName"); OnPropertyChanged("BackupTypeImage"); } }

        public Guid GUID { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get { return Name; } set { } }

        IStorageInterface m_IStorage = new FileSystemStorage();
        public IStorageInterface GetStorageInterface() { return m_IStorage; }

        private string m_TargetBackupFolder;
        public string TargetBackupFolder { get { return m_TargetBackupFolder; } set { m_TargetBackupFolder = value; OnPropertyChanged(); } }


        private string m_TargetRestoreFolder;
        public string TargetRestoreFolder { get { return m_TargetRestoreFolder; } set { m_TargetRestoreFolder = value; OnPropertyChanged(); } }

        public ObservableCollection<FolderData> BackupFolderList { get; set; } = new ObservableCollection<FolderData>();

        public ObservableCollection<WatcherItemData> BackupWatcherItemList { get; set; } = new ObservableCollection<WatcherItemData>();

        [XmlIgnore]
        public long BackupWatcherItemListCount { get { return BackupWatcherItemList.Count(); } set { } }


        public void ClearBackupFolderList()
        {
            //clear only available items
            var availableItems = BackupFolderList.Where(i => i.IsAvailable == true).ToArray();
            if (availableItems != null)
            {
                foreach (var item in availableItems)
                {
                    BackupFolderList.Remove(item);
                }
            }
            //BackupFolderList.Clear();
        }

        [XmlIgnore]
        public ProfileDataRefreshTask ProfileDataRefreshTask { get; private set; }

        [XmlIgnore]
        public ObservableCollection<FolderData> RestoreFolderList { get; set; } = new ObservableCollection<FolderData>();

        [XmlIgnore]
        public bool IsValidProfileFolder { get { return GetProfileTargetFolderStatus(m_TargetBackupFolder) != ProfileTargetFolderStatusEnum.AssosiatedWithThisProfile; } }

        [XmlIgnore]
        public ProfileTargetFolderStatusEnum ProfileTargetFolderStatus { get { return GetProfileTargetFolderStatus(m_TargetBackupFolder); } }


        [XmlIgnore]
        public BackupWorker BackupWorkerTask { get; set; }

        [XmlIgnore]
        public bool IsBackupWorkerBusy { get { return BackupWorkerTask != null && BackupWorkerTask.IsBusy; } set { OnPropertyChanged(); } }

        [XmlIgnore]
        public bool IsBackupWorkerPaused { get { return BackupWorkerTask != null && BackupWorkerTask.IsPaused == true; } set { BackupWorkerTask.IsPaused = value; OnPropertyChanged(); } }


        public void StartBackup(bool bFullBackupScan)
        {
            if (BackupWorkerTask != null && BackupWorkerTask.IsBusy)
            {
                //busy
            }
            else
            {
                BackupWorkerTask = new BackupWorker(this, bFullBackupScan);
                BackupWorkerTask.RunWorkerAsync();
            }
        }

        public void PauseBackup()
        {
            if (BackupWorkerTask != null && BackupWorkerTask.IsBusy)
            {
                IsBackupWorkerPaused = true;
            }
        }
        public void ResumeBackup()
        {
            if (BackupWorkerTask != null && BackupWorkerTask.IsBusy)
            {
                IsBackupWorkerPaused = false;
            }
        }


        public void SetBackupType(BackupTypeEnum backupType)
        {
            if (BackupType != backupType)
            {

            }
        }

        //private ProfileTargetFolderStatusEnum m_ProfileTargetFolderStatus = ProfileTargetFolderStatusEnum.InvalidTargetPath;
        public ProfileTargetFolderStatusEnum GetProfileTargetFolderStatus(string path)
        {
            ProfileTargetFolderStatusEnum profileStatus;

            if (!m_IStorage.DirectoryExists(path))
            {
                profileStatus = ProfileTargetFolderStatusEnum.InvalidTargetPath;
            }
            else
            {
                var subdirectoryList = GetDirectoriesNames(path);

                if (subdirectoryList == null || subdirectoryList.Count() == 0)
                {
                    profileStatus = ProfileTargetFolderStatusEnum.EmptyFolderNoProfile;
                }
                else
                {
                    int iMatchCount = 0;
                    int iOtherMatchCount = 0;
                    foreach (string subDirectory in subdirectoryList)
                    {
                        string newPath = m_IStorage.Combine(path, subDirectory);
                        if (m_IStorage.IsFolder(newPath))
                        {
                            if (subDirectory.StartsWith(GUID.ToString("D")))
                            {
                                iMatchCount++;
                            }
                            else
                            {
                                try
                                {
                                    if (subDirectory.Length > 36)
                                    {
                                        Guid newGuid = Guid.Parse(subDirectory.Substring(0, 32 + 4));
                                        iOtherMatchCount++;
                                    }
                                }
                                catch (ArgumentNullException)
                                {
                                    Console.WriteLine("The string to be parsed is null.");
                                }
                                catch (FormatException)
                                {
//                                    Console.WriteLine("Bad format: {0}", subDirectory);
                                }
                            }
                        }
                    }

                    if (iMatchCount == subdirectoryList.Count)
                    {
                        profileStatus = ProfileTargetFolderStatusEnum.AssosiatedWithThisProfile;
                    }
                    else if (iMatchCount == 0 && iOtherMatchCount == 0)
                    {
                        profileStatus = ProfileTargetFolderStatusEnum.NonEmptyFolderNoProfile;
                    }
                    else if (iOtherMatchCount > 0)
                    {
                        profileStatus = ProfileTargetFolderStatusEnum.AssosiatedWithADifferentProfile;
                    }
                    else
                    {
                        profileStatus = ProfileTargetFolderStatusEnum.CoccuptedOrNotRecognizedProfile;
                    }
                }
            }

            return profileStatus;
        }

        public int ConverBackupProfileFolderToNewPath(string path)
        {
            int iCount = -1;

            var subdirectoryList = GetDirectoriesNames(path);
            if (subdirectoryList == null || subdirectoryList.Count() == 0)
            {
                return 0;
            }
            else
            {
                iCount = 0;
                foreach (string subDirectory in subdirectoryList)
                {
                    string sourcePath = m_IStorage.Combine(path, subDirectory);
                    if (m_IStorage.IsFolder(sourcePath) && (subDirectory.Length >= 36))
                    {
                        try
                        {
                            Guid newGuid = Guid.Parse(subDirectory.Substring(0, 36));
                            if (GUID != newGuid)
                            {
                                var sub1 = subDirectory.Substring(36, subDirectory.Length - 36);
                                var sub2 = subDirectory.Remove(36);

                                var destDirectory = GUID.ToString("D") + subDirectory.Substring(36, subDirectory.Length - 36);
                                string targetPath = m_IStorage.Combine(path, destDirectory);
                                if (m_IStorage.MoveDirectory(sourcePath, targetPath))
                                {
                                    iCount++;
                                }
                            }
                        }
                        catch (ArgumentNullException)
                        {
                            Console.WriteLine("The string to be parsed is null.");
                        }
                        catch (FormatException)
                        {
                            //                                    Console.WriteLine("Bad format: {0}", subDirectory);
                        }
                    }
                }
            }

            return iCount;
        }



        public List<string> GetDirectoriesNames(string path)
        {

            //Process directories
            string[] sourceSubdirectoryEntries = m_IStorage.GetDirectories(path);
            var sourceSubdirectoryEntriesList = new List<string>();
            if (sourceSubdirectoryEntries != null)
            {
                if (sourceSubdirectoryEntriesList != null)
                {
                    foreach (var entry in sourceSubdirectoryEntries)
                    {
                        sourceSubdirectoryEntriesList.Add(m_IStorage.GetFileName(entry));
                    }
                }
            }

            return sourceSubdirectoryEntriesList;
        }



        public void UpdateProfileProperties()
        {
            if (ProfileDataRefreshTask?.IsBusy == false)
            {
                ProfileDataRefreshTask.RunWorkerAsync();
            }                
        }

        public long BackupTargetDiskSizeNumber { get; set; } = 0;
        private string m_BackupTargetDiskSize = "Data Not available";
        public string BackupTargetDiskSize { get { return m_BackupTargetDiskSize; } set { m_BackupTargetDiskSize = value; OnPropertyChanged(); } }

        public long BackupTargetUsedSizeNumber { get; set; } = 0;
        private string m_BackupTargetUsedSize = "Data Not available";
        public string BackupTargetUsedSize { get { return m_BackupTargetUsedSize; } set { m_BackupTargetUsedSize = value; OnPropertyChanged(); } }

        public long BackupTargetFreeSizeNumber { get; set; } = 0;
        private string m_BackupTargetFreeSize = "Data Not available";
        public string BackupTargetFreeSize { get { return m_BackupTargetFreeSize; } set { m_BackupTargetFreeSize = value; OnPropertyChanged(); } }


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
