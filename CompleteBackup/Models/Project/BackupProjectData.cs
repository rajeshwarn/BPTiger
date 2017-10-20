using CompleteBackup.Models.Backup.Profile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models.Backup.Project
{
    public class BackupProjectData : ObservableObject
    {
        public string Name { get; set; } = "My Project";
        public string Description { get { return Name; } set { } }

        private Guid CurrentProfileGuid { get; set; } = Guid.Empty;

        public BackupProfileData CurrentBackupProfile { get { return BackupProfileList.FirstOrDefault(p => p.GUID == CurrentProfileGuid); }
                                                        set { CurrentProfileGuid = value.GUID; OnPropertyChanged(); } }

        public ObservableCollection<BackupProfileData> BackupProfileList { get; set; } = new ObservableCollection<BackupProfileData>();
    }
}
