using CompleteBackup.Models.Profile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CompleteBackup.Models.Backup.Profile
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

    //public enum BackupRunTypeEnum
    //{
    //    Always,
    //    Manual,
    //}
    public enum BackupTypeEnum
    {
        Snapshot,
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
        public static Dictionary<ProfileTargetFolderStatusEnum, string> ProfileTargetFolderStatusDictionary { get; } = new Dictionary<ProfileTargetFolderStatusEnum, string>()
        {
            { ProfileTargetFolderStatusEnum.AssosiatedWithADifferentProfile, "The folder is associated with a different backup profile" },
            { ProfileTargetFolderStatusEnum.AssosiatedWithThisProfile, "" },
            { ProfileTargetFolderStatusEnum.CoccuptedOrNotRecognizedProfile, "The backup profile in this folder is not recognized or corrupted" },
            { ProfileTargetFolderStatusEnum.EmptyFolderNoProfile, "" },
            { ProfileTargetFolderStatusEnum.InvalidTargetPath, "The folder does not exist or invalid" },
            { ProfileTargetFolderStatusEnum.NonEmptyFolderNoProfile, "The folder does not contain a backup profile" },
        };

        public static List<BackupTypeData> BackupTypeList
        {
            get
            {
                return new List<BackupTypeData>()
                {
                    new BackupTypeData() { BackupType = BackupTypeEnum.Snapshot, Name = "Snapshot Backup", ImageName = @"/Resources/Icons/Ribbon/FullBackup.ico",
                        Description = "Creates a full copy of your source items and any subsequent backups will copy all source items again into a separate backup folder." +
                        "\nThis is recomended if you want to create an identical copy of your source items and keep it somewhere safe." },

                    new BackupTypeData() { BackupType = BackupTypeEnum.Incremental, Name = "Incremental Backup", ImageName = @"/Resources/Icons/Ribbon/IncrementalBackup.ico",
                        Description = "Starts with a full copy of your items, Subsequent copies will copy only items that have been changes.\nThis method is usually faster than Snapshot, however you will not be able to restore older item versions" +
                        ", since this method does not keep any history, Because of that this should be called mirror or copy and not a backup" },

                    new BackupTypeData() { BackupType = BackupTypeEnum.Differential, Name = "Differential Backup", ImageName = @"/Resources/Icons/Ribbon/DifferentialBackup.ico",
                        Description = "Similar to Incremental Backup, but also keeps all the items that have changed or deleted" +
                        "\nThis is the preffered backup method, This method works the best it lets you to restore an old items have been changed or deleted in the past" +
                        ", however keeping old veriosns over time might consume an extra storage space, when using this methiod it is recomended to check the storage usage and delete old history if no longer needed", IsChecked = true},
                };
            }
        }
    }


    public static class ProfileDataExtensionMethods
    {
        public static void InitProfile(this BackupProfileData profile)
        {
            if (profile.GetTargetRestoreFolder() == null)
            {
                //Add default restore folder if not exists
                profile.TargetRestoreFolderList.Add(new FolderData() { IsFolder = true, IsAvailable = true, Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) });
            }

            profile.ProfileDataRefreshTask = new ProfileDataRefreshWorkerTask(profile);
            profile.RefreshProfileProperties();

            if (profile.BackupType != BackupTypeEnum.Snapshot)
            {
                profile.FileSystemWatcherWorker = FileSystemWatcherWorkerTask.StartNewInstance(profile, profile.UpdateWatchItemsTimeSeconds * 1000);
            }

            if (profile.IsBackupSleep)
            {
                BackupAlertManager.Instance.AddAlert(profile, BackupPerfectAlertTypeEnum.BackupInSleepMode, $", Wakeup time: {profile.WateUpFromSleepTime}");
            }

            if (!profile.IsWatchFileSystem)
            {
                BackupAlertManager.Instance.AddAlert(profile, BackupPerfectAlertTypeEnum.BackupFileSystemWatcherNotRunning);
            }

            profile.UpdateAlerts();
        }

        public static string GetTargetBackupFolder(this BackupProfileData profile)
        {
            string path = null;
            var folder = profile.GetTargetBackupFolderData();
            if (folder != null)
            {
                path = folder.Path;//profile.GetStorageInterface().Combine(folder.Path, BackupProfileData.TargetBackupBaseDirectoryName);
            }

            return path;
        }
        public static FolderData GetTargetBackupFolderData(this BackupProfileData profile)
        {
            return profile?.TargetBackupFolderList?.FirstOrDefault();
        }


        public static string GetTargetRestoreFolder(this BackupProfileData profile)
        {
            string path = null;
            var folder = profile?.TargetRestoreFolderList?.FirstOrDefault();
            if (folder != null)
            {
                path = folder.Path;
            }

            return path;
        }

        public static bool IsValidFolderName(this BackupProfileData profile, string path)
        {
            return (path != null) && (path != string.Empty);
        }

        public static void AddItemToBackupWatcherItemList(this BackupProfileData profile, FileSystemWatcherItemData item)
        {
            lock (profile)
            {
                profile.BackupWatcherItemList.Add(item);
            }
        }

        public static void ClearBackupFolderList(this BackupProfileData profile)
        {
            //clear only available items
            var availableItems = profile.BackupFolderList.Where(i => i.IsAvailable == true).ToArray();
            if (availableItems != null)
            {
                foreach (var item in availableItems)
                {
                    profile.BackupFolderList.Remove(item);
                }
            }
        }

        public static void RefreshProfileProperties(this BackupProfileData profile)
        {
            lock (profile)
            {
                if (profile.ProfileDataRefreshTask == null)
                {
                    profile.ProfileDataRefreshTask = new ProfileDataRefreshWorkerTask(profile);
                }

                profile.ProfileDataRefreshTask.StartTask();
            }
        }

        public static void SleepBackup(this BackupProfileData profile, int hours)
        {
            if (hours == 0)
            {
                profile.WateUpFromSleepTime = DateTime.Now;

                //                OnPropertyChanged("IsBackupSleep");
                profile.IsBackupSleep = false;

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    BackupAlertManager.Instance.RemoveAlert(profile, BackupPerfectAlertTypeEnum.BackupInSleepMode);
                }));
            }
            else
            {
                DateTime timeNow = DateTime.Now;

                profile.WateUpFromSleepTime = timeNow.AddHours(hours);

                //                OnPropertyChanged("IsBackupSleep");
                profile.IsBackupSleep = true;

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    BackupAlertManager.Instance.AddAlert(profile, BackupPerfectAlertTypeEnum.BackupInSleepMode, $", Wakeup time: {profile.WateUpFromSleepTime}");
                }));
            }
        }

        public static ProfileTargetFolderStatusEnum GetProfileTargetFolderStatus(this BackupProfileData profile, string path)
        {
            ProfileTargetFolderStatusEnum profileStatus;

            if (!profile.GetStorageInterface().DirectoryExists(path))
            {
                profileStatus = ProfileTargetFolderStatusEnum.InvalidTargetPath;
            }
            else
            {
                var subdirectoryList = GetDirectoriesNames(profile, path);

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
                        string newPath = profile.GetStorageInterface().Combine(path, subDirectory);
                        if (profile.GetStorageInterface().IsFolder(newPath))
                        {
                            if (subDirectory.StartsWith(profile.GUID.ToString("D")))
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



        public static int ConverBackupProfileFolderToNewPath(this BackupProfileData profile, string path)
        {
            int iCount = -1;

            var subdirectoryList = GetDirectoriesNames(profile, path);
            if (subdirectoryList == null || subdirectoryList.Count() == 0)
            {
                return 0;
            }
            else
            {
                iCount = 0;
                foreach (string subDirectory in subdirectoryList)
                {
                    string sourcePath = profile.GetStorageInterface().Combine(path, subDirectory);
                    if (profile.GetStorageInterface().IsFolder(sourcePath) && (subDirectory.Length >= 36))
                    {
                        try
                        {
                            Guid newGuid = Guid.Parse(subDirectory.Substring(0, 36));
                            if (profile.GUID != newGuid)
                            {
                                var sub1 = subDirectory.Substring(36, subDirectory.Length - 36);
                                var sub2 = subDirectory.Remove(36);

                                var destDirectory = profile.GUID.ToString("D") + subDirectory.Substring(36, subDirectory.Length - 36);
                                string targetPath = profile.GetStorageInterface().Combine(path, destDirectory);
                                if (profile.GetStorageInterface().MoveDirectory(sourcePath, targetPath))
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

        public static List<string> GetDirectoriesNames(this BackupProfileData profile, string path)
        {

            //Process directories
            string[] sourceSubdirectoryEntries = profile.GetStorageInterface().GetDirectories(path);
            var sourceSubdirectoryEntriesList = new List<string>();
            if (sourceSubdirectoryEntries != null)
            {
                if (sourceSubdirectoryEntriesList != null)
                {
                    foreach (var entry in sourceSubdirectoryEntries)
                    {
                        sourceSubdirectoryEntriesList.Add(profile.GetStorageInterface().GetFileName(entry));
                    }
                }
            }

            return sourceSubdirectoryEntriesList;
        }
    }

    public class BackupProfileManager
    {
       // public BackupProfileData ProfileData { get; set; }

        public BackupProfileManager() { }

        public static BackupProfileData CreateBackupProfileData(string name, BackupTypeEnum backupType = BackupTypeEnum.Differential, string description = null)
        {
            var profile = new BackupProfileData()
            {
                BackupType = backupType,
                Name = name,
                Description = description,
                BackupFolderList = new ObservableCollection<FolderData>(),
            };

            return profile;
        }







    }
}
