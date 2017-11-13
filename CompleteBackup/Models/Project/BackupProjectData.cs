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

        private BackupProfileData m_CurrentBackupProfile;
        [XmlIgnore]
        public BackupProfileData CurrentBackupProfile
        {
            get
            {
                if (m_CurrentBackupProfile == null) { m_CurrentBackupProfile = BackupProfileList.FirstOrDefault(p => p.GUID == CurrentProfileGuid); }

                return m_CurrentBackupProfile;
            }

            set {
                var prevProfile = m_CurrentBackupProfile;

                m_CurrentBackupProfile = value;
                CurrentProfileGuid = value.GUID;

                //This will trigger the UI to draw selected item/color
                if (prevProfile != null) { prevProfile.IsCurrentProfileSelected = false; }
                value.IsCurrentProfileSelected = true;

                OnPropertyChanged();
            }
        }

        public ObservableCollection<BackupProfileData> BackupProfileList { get; set; } = new ObservableCollection<BackupProfileData>();
    }
}
