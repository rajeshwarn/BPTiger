using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CompleteBackup.Models.Backup.Project
{
    public class BackupProjectData : ObservableObject
    {
        public BackupProjectData()
        {            
        }

        public void Init()
        {
            foreach (var profile in BackupProfileList)
            {
                profile.Init();
            }
        }

        [XmlIgnore]
        public BackupPerfectLogger Logger { get; } = new BackupPerfectLogger();

        public string Name { get; set; } = "My Project";
        public string Description { get { return Name; } set { } }

        public Guid CurrentProfileGuid { get; set; } = Guid.Empty;

        [XmlIgnore]
        public BackupProfileData CurrentBackupProfile {
            get { return BackupProfileList.FirstOrDefault(p => p.GUID == CurrentProfileGuid); }

            set {
                var lastProfile = BackupProfileList.FirstOrDefault(p => p.GUID == CurrentProfileGuid);

                if (value == null)
                {
                    CurrentProfileGuid = Guid.Empty;
                }
                else
                {
                    CurrentProfileGuid = value.GUID;
                    value.IsCurrentProfileSelected = true; //this will trigger OnPropertyChange
                }

                //only after we changed CurrentProfileGuid, we cann set IsCurrent value
                if (lastProfile != null)
                {
                    lastProfile.IsCurrentProfileSelected = false; //this will trigger OnPropertyChange
                }

                OnPropertyChanged();
            }
        }

        public ObservableCollection<BackupProfileData> BackupProfileList { get; set; } = new ObservableCollection<BackupProfileData>();
    }
}
