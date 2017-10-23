using CompleteBackup.Models.Backup.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace CompleteBackup.Models.Backup.Profile
{
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

        public Guid GUID { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "My Backup Profile";
        public string Description { get { return Name; } set { } }

        IStorageInterface m_IStorage = new FileSystemStorage();

        public ObservableCollection<string> FolderList { get; set; } = new ObservableCollection<string>();

        [XmlIgnore]
        public bool IsValidProfileFolder { get { return GetProfileTargetFolderStatus(_TargetBackupFolder) != ProfileTargetFolderStatusEnum.AssosiatedWithThisProfile; } }

        [XmlIgnore]
        public ProfileTargetFolderStatusEnum ProfileTargetFolderStatus { get { return GetProfileTargetFolderStatus(_TargetBackupFolder); } }

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

                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        try
                        {
                            Guid newGuid = Guid.Parse(subDirectory.Substring(0, 36));

                            var sub1 = subDirectory.Substring(36, subDirectory.Length - 36);
                            var sub2 = subDirectory.Remove(36);

                            var destDirectory = GUID.ToString("D") + subDirectory.Substring(36, subDirectory.Length - 36);
                            string targetPath = m_IStorage.Combine(path, destDirectory);
                            if (m_IStorage.MoveDirectory(sourcePath, targetPath))
                            {
                                iCount++;
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


        private string _TargetBackupFolder;
        public string TargetBackupFolder { get { return _TargetBackupFolder; } set  { _TargetBackupFolder = value; OnPropertyChanged(); } }

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
                        BackupTargetUsedSize = "n/a";
                        BackupTargetFreeSize = "n/a";
                        BackupSourceFoldersSize = "n/a";
                    }));

                    m_BackupTargetFreeSizeNumber = 0;
                    //Target Total Space
                    if (_TargetBackupFolder != null)
                    {
                        m_BackupTargetFreeSizeNumber = new DirectoryInfo(_TargetBackupFolder).GetFiles("*.*", SearchOption.AllDirectories).Sum(file => file.Length);
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            BackupTargetUsedSize = (m_BackupTargetFreeSizeNumber / 1000000).ToString("###,##0") + " KBytes";
                        }));
                    }

                    //Target free space
                    m_BackupTargetFreeSizeNumber = 0;
                    if (_TargetBackupFolder != null)
                    {
                        string drive1 = Path.GetPathRoot(_TargetBackupFolder);
                        foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.ToString().Contains(drive1)))
                        {
                            if (drive.IsReady)
                            {
                                m_BackupTargetFreeSizeNumber = drive.TotalFreeSpace;
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    BackupTargetFreeSize = (m_BackupTargetFreeSizeNumber / 1000000).ToString("###,##0") + " KBytes";
                                }));

                                break;
                            }
                        }
                    }

                    //Source Foldes Size
                    foreach(var folder in FolderList)
                    {
                        m_BackupSourceFoldersSizeNumber += new DirectoryInfo(folder).GetFiles("*.*", SearchOption.AllDirectories).Sum(file => file.Length);
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            BackupSourceFoldersSize = (m_BackupSourceFoldersSizeNumber / 1000000).ToString("###,##0") + " KBytes";
                        }));
                    }


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


        private long m_BackupTargetUsedSizeNumber = 0;
        private string m_BackupTargetUsedSize = "Data Not available";
        public string BackupTargetUsedSize { get { return m_BackupTargetUsedSize; } set { m_BackupTargetUsedSize = value; OnPropertyChanged(); } }


        private long m_BackupTargetFreeSizeNumber = 0;
        private string m_BackupTargetFreeSize = "Data Not available";
        public string BackupTargetFreeSize { get { return m_BackupTargetFreeSize; } set { m_BackupTargetFreeSize = value; OnPropertyChanged(); } }

        private long m_BackupSourceFoldersSizeNumber = 0;
        private string m_BackupSourceFoldersSize = "Data Not available";
        public string BackupSourceFoldersSize { get { return m_BackupSourceFoldersSize; } set { m_BackupSourceFoldersSize = value; OnPropertyChanged(); } }
    }
}
