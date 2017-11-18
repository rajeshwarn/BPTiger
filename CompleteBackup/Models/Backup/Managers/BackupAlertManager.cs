using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models.Backup
{
    public class BackupPerfectAlertData
    {
        public DateTime AlertTime { get; set; }
        public BackupPerfectAlertSourceEnum BackupPerfectAlertSource { get; set; }
        public BackupPerfectAlertTypeEnum AlertType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageName { get; set; }
    }

    public enum BackupPerfectAlertTypeEnum
    {
        Notification,
        Warning,
        Error,
    }
    public enum BackupPerfectAlertSourceEnum
    {
        Generic,
        Backup,
        Restore,
    }

    public enum BackupPerfectAlertValueEnum
    {
    
    }


    public class BackupAlertManager
    {
        BackupProfileData m_Profile;
        BackupPerfectLogger m_Logger;

        BackupAlertManager() { }

        public BackupAlertManager(BackupProfileData profile)
        {
            m_Profile = profile;
            m_Logger = profile.Logger;
        }

        public static BackupAlertManager Instance { get; } = new BackupAlertManager();



    }
}
