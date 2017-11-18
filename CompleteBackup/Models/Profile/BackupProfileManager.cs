using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models.Backup.Profile
{
    public enum BackupRunTypeEnum
    {
        Always,
        Manual,
    }
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

    public class BackupProfileManager
    {
        public BackupProfileData ProfileData { get; set; }

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
