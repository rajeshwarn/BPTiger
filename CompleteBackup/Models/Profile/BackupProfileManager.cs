using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models.Backup.Profile
{
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
