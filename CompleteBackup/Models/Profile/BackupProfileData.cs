﻿using CompleteBackup.DataRepository;
using CompleteBackup.Models.backup;
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
    public enum BackupTypeEnum
    {
        Full,
        Incremental,
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
            InitStorageDataUpdaterTask();
        }


        public string BackupSignature { get { return $"{GUID.ToString("D")}-BC-{BackupType}"; } }


        public BackupTypeEnum BackupType { get; set; } = BackupTypeEnum.Full;
        public Guid GUID { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "My Backup Profile";
        public string Description { get { return Name; } set { } }

        IStorageInterface m_IStorage = new FileSystemStorage();
        public IStorageInterface GetStorageInterface() { return m_IStorage; }

        private string m_TargetBackupFolder;
        public string TargetBackupFolder { get { return m_TargetBackupFolder; } set { m_TargetBackupFolder = value; OnPropertyChanged(); } }

        public ObservableCollection<FolderData> FolderList { get; set; } = new ObservableCollection<FolderData>();
        public ObservableCollection<FolderData> RestoreFolderList { get; set; } = new ObservableCollection<FolderData>();

        //[XmlIgnore]
        //public ObservableCollection<FolderMenuItem> RootFolderItemList { get; set; } = new ObservableCollection<FolderMenuItem>();

        public delegate void ProfileDataUpdateEvent(BackupProfileData tranData);
        public event ProfileDataUpdateEvent m_ProfileDataUpdateEventCallback;

        public void RegisterEvent(ProfileDataUpdateEvent callback)
        {
            m_ProfileDataUpdateEventCallback += callback;
        }

        public void UnRegisterEvent(ProfileDataUpdateEvent callback)
        {
            m_ProfileDataUpdateEventCallback -= callback;
        }


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


        public void StartBackup()
        {
            if (BackupWorkerTask != null && BackupWorkerTask.IsBusy)
            {
                //busy
            }
            else
            {
                BackupWorkerTask = new BackupWorker(this);
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
                        FileAttributes attr = m_IStorage.GetFileAttributes(newPath);

                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
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
                    FileAttributes attr = m_IStorage.GetFileAttributes(sourcePath);

                    if (((attr & FileAttributes.Directory) == FileAttributes.Directory) &&
                        (subDirectory.Length >= 36))
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


        //==================================

        BackgroundWorker m_StorageDataUpdaterTask = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

        

        void InitStorageDataUpdaterTask()
        {
            m_StorageDataUpdaterTask.DoWork += (sender, e) =>
            {
                //var collection = e.Argument as EventCollectionDataBase;
                try
                {

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        BackupTargetDiskSize = "n/a";
                        BackupTargetUsedSize = "n/a";
                        BackupTargetFreeSize = "n/a";
                    }));

                    //Source folders sizes and number of files
                    BackupSourceFilesNumber = 0;
                    BackupSourceFoldersSize = 0;
                    foreach (var item in FolderList)
                    {
                        if (item.IsFolder)
                        {
                            item.NumberOfFiles = new DirectoryInfo(item.Path).GetFiles("*.*", SearchOption.AllDirectories).Sum(file => 1);
                            item.TotalSize = new DirectoryInfo(item.Path).GetFiles("*.*", SearchOption.AllDirectories).Sum(file => file.Length);
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                BackupSourceFilesNumber += item.NumberOfFiles;
                                BackupSourceFoldersSize += item.TotalSize;
                            }));
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                BackupSourceFilesNumber++;
                            }));
                        }
                    }


                    //Restore folders sizes and number of files
                    RestoreSourceFilesNumber = 0;
                    RestoreSourceFoldersSize = 0;
                    foreach (var item in RestoreFolderList)
                    {
                        if (item.IsFolder)
                        {
                            item.NumberOfFiles = new DirectoryInfo(item.Path).GetFiles("*.*", SearchOption.AllDirectories).Sum(file => 1);
                            item.TotalSize = new DirectoryInfo(item.Path).GetFiles("*.*", SearchOption.AllDirectories).Sum(file => file.Length);
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                RestoreSourceFilesNumber += item.NumberOfFiles;
                                RestoreSourceFoldersSize += item.TotalSize;
                            }));
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                RestoreSourceFilesNumber++;
                            }));
                        }
                    }











                    m_BackupTargetDiskSizeNumber = 0;
                    //Target disk size
                    if (m_TargetBackupFolder != null)
                    {
                        string rootDrive = Path.GetPathRoot(m_TargetBackupFolder);
                        foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.ToString().Contains(rootDrive)))
                        {
                            if (drive.IsReady)
                            {
                                m_BackupTargetDiskSizeNumber = drive.TotalSize;
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    BackupTargetDiskSize = FileFolderSizeHelper.GetNumberSizeString(m_BackupTargetDiskSizeNumber);
                                }));

                                break;
                            }
                        }
                    }

                    m_BackupTargetUsedSizeNumber = 0;
                    //Target used Space
                    if (m_TargetBackupFolder != null)
                    {
                        m_BackupTargetUsedSizeNumber = new DirectoryInfo(m_TargetBackupFolder).GetFiles("*.*", SearchOption.AllDirectories).Sum(file => file.Length);
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            BackupTargetUsedSize = FileFolderSizeHelper.GetNumberSizeString(m_BackupTargetUsedSizeNumber);
                        }));
                    }

                    //Target free space
                    m_BackupTargetFreeSizeNumber = 0;
                    if (m_TargetBackupFolder != null)
                    {
                        string rootDrive = Path.GetPathRoot(m_TargetBackupFolder);
                        foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.ToString().Contains(rootDrive)))
                        {
                            if (drive.IsReady)
                            {
                                m_BackupTargetFreeSizeNumber = drive.AvailableFreeSpace;
                                //m_BackupTargetFreeSizeNumber = drive.TotalFreeSpace;
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    BackupTargetFreeSize = FileFolderSizeHelper.GetNumberSizeString(m_BackupTargetFreeSizeNumber);
                                }));

                                break;
                            }
                        }
                    }

                    ////Source Foldes Size
                    //m_BackupSourceFoldersSizeNumber = 0;
                    //foreach (var item in FolderList)
                    //{
                    //    m_BackupSourceFoldersSizeNumber += new DirectoryInfo(item.Path).GetFiles("*.*", SearchOption.AllDirectories).Sum(file => file.Length);
                    //    Application.Current.Dispatcher.Invoke(new Action(() =>
                    //    {
                    //        BackupSourceFoldersSize = FileFolderSizeHelper.GetNumberSizeString(m_BackupSourceFoldersSizeNumber);
                    //    }));
                    //}

                    m_ProfileDataUpdateEventCallback(this);
                    
                    //update data to persistent storage
                    BackupProjectRepository.Instance.SaveProject();
                }
                catch (TaskCanceledException ex)
                {
                    Trace.WriteLine($"StorageDataUpdaterTask exception: {ex.Message}");
                    e.Result = $"StorageDataUpdaterTaskexception: {ex.Message}";
                    throw (ex);
                }
            };
        }


        public void UpdateProfileProperties()
        {
            if (!m_StorageDataUpdaterTask.IsBusy)
            {
                m_StorageDataUpdaterTask.RunWorkerAsync();
            }                
        }

        public long m_BackupTargetDiskSizeNumber { get; set; } = 0;
        private string m_BackupTargetDiskSize = "Data Not available";
        public string BackupTargetDiskSize { get { return m_BackupTargetDiskSize; } set { m_BackupTargetDiskSize = value; OnPropertyChanged(); } }

        public long m_BackupTargetUsedSizeNumber { get; set; } = 0;
        private string m_BackupTargetUsedSize = "Data Not available";
        public string BackupTargetUsedSize { get { return m_BackupTargetUsedSize; } set { m_BackupTargetUsedSize = value; OnPropertyChanged(); } }

        public long m_BackupTargetFreeSizeNumber { get; set; } = 0;
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
