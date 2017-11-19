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
                profile.InitProfile();
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

                if (value == null)
                {
                    CurrentProfileGuid = Guid.Empty;
                    if (prevProfile != null) { prevProfile.IsCurrentProfileSelected = false; }
                }
                else
                {
                    CurrentProfileGuid = value.GUID;

                    //This will trigger the UI to draw selected item/color
                    if (prevProfile != null) { prevProfile.IsCurrentProfileSelected = false; }
                    value.IsCurrentProfileSelected = true;
                }

                OnPropertyChanged();
                m_CurrentProfileChangeEventCallback(value);
            }
        }

        public ObservableCollection<BackupProfileData> BackupProfileList { get; set; } = new ObservableCollection<BackupProfileData>();



        public delegate void CurrentProfileChangeEvent(BackupProfileData profile);
        private event CurrentProfileChangeEvent m_CurrentProfileChangeEventCallback;

        public void RegisterProfileChangeEvent(CurrentProfileChangeEvent callback)
        {
            m_CurrentProfileChangeEventCallback += callback;
        }

        public void UnRegisterProfileChangeEvent(CurrentProfileChangeEvent callback)
        {
            m_CurrentProfileChangeEventCallback -= callback;
        }
    }
}
