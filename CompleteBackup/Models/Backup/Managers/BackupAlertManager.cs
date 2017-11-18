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
using System.Windows;

namespace CompleteBackup.Models.Backup
{
    public class BackupPerfectAlertData
    {
        public BackupPerfectAlertTypeEnum AlertType { get; set; }
        
        public DateTime AlertTime { get; set; }
        public BackupPerfectAlertSourceEnum BackupPerfectAlertSource { get; set; }
        public BackupPerfectNotificationTypeEnum NotificationType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageName { get; set; }
    }

    public enum BackupPerfectNotificationTypeEnum
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

    public enum BackupPerfectAlertTypeEnum
    {
        BackupItemListEmpty,
        RestoreItemListEmpty,
        BackupItemListFolderNotAvailable,
        BackupItemListFileNotAvailable,

        BackupDestinationFolderNotConfigured,
        RestoreDestinationFolderNotConfigured,
        BackupDestinationFolderNotAvailable,
        RestoreDestinationFolderNotAvailable,

        BackupDestinationNotFound, //folder available no file

        BackupInSleepMode,

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

        public static Dictionary<BackupPerfectAlertTypeEnum, BackupPerfectAlertData> BackupPerfectAlertValueDictionary { get; } = new Dictionary<BackupPerfectAlertTypeEnum, BackupPerfectAlertData>()
        {
            {
                BackupPerfectAlertTypeEnum.BackupInSleepMode,
                new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.BackupInSleepMode,
                    Name = $"Back up in sleep mode",
                    Description = "Sleep mode prevents from being backed up, during this mode automatic or scheduled backup will not run, While can wakeup from sleep mode anytime by pressing on the \"Wakeup\" button or wait for the sleep time to expire"
                }
            },

            {
                BackupPerfectAlertTypeEnum.BackupItemListEmpty,
                new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.BackupItemListEmpty,
                    Name = $"Back up item list is empty",
                    Description = "You must select atleast one backup item, item can be a file or a folder, if the selected item is folder the entire folder content will be backup includeing all sub items. " +
                                "To select items select in the \"Items to backup\" from Setting or press on \"Select items to backup button\""
                }
            },

            {
                BackupPerfectAlertTypeEnum.BackupItemListFolderNotAvailable,
                new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.BackupItemListFolderNotAvailable,
                    Name = $"Backup directory is not available",
                }
            },

            {
                BackupPerfectAlertTypeEnum.BackupItemListFileNotAvailable,
                new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.BackupItemListFileNotAvailable,
                    Name = $"Backup file is not available",
                }
            },

            {
                BackupPerfectAlertTypeEnum.BackupDestinationFolderNotConfigured,
                new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.BackupDestinationFolderNotConfigured,
                    Name = $"Destination backup directory is not set",
                    Description = "Destination backp directory is the destination directory where the backup files will be stored. Please select a destination back directory, it is recomended to select an empty directory",
                }
            },

            {
                BackupPerfectAlertTypeEnum.BackupDestinationFolderNotAvailable,
                new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.BackupDestinationFolderNotAvailable,
                    Name = $"Destination backup directory is not available",
                    Description = "Destination backp directory is the destination directory where the backup files will be stored, it is recomended to select an empty directory",
                }
            },

            {
                BackupPerfectAlertTypeEnum.BackupDestinationNotFound,
                new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.BackupDestinationNotFound,
                    Name = $"Backup not found, nothing to restore",
                    Description = "Could not find the destination backup.  This usually happens if the destination backup storage is not available or has not been running" ,
                }
            },

            {
                BackupPerfectAlertTypeEnum.RestoreItemListEmpty,
                new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.RestoreItemListEmpty,
                    Name = $"Restore item list is empty",
                    Description = "Please select at least one item to restore",
                }
            },

            {
                BackupPerfectAlertTypeEnum.RestoreDestinationFolderNotAvailable,
                new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.RestoreDestinationFolderNotAvailable,
                    Name = $"Restore directory is not available",
                    Description = "Please select a valid destination directory to store the restored files",
                }
            },

            {
                BackupPerfectAlertTypeEnum.RestoreDestinationFolderNotConfigured,
                new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.RestoreDestinationFolderNotConfigured,
                    Name = $"Restore directory is not configured",
                    Description = "Please select a destination directory to store the restored files",
                }
            },


        };


        public void AddAlert(BackupProfileData profile, BackupPerfectAlertTypeEnum alert, string text = null)
        {
            var alertData = BackupPerfectAlertValueDictionary[alert];
            if (text != null)
            {
                alertData.Name += $" :{text}";
            }

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (alert.ToString().StartsWith("Restore"))
                {
                    profile.RestoreAlertList.Add(alertData);
                }
                else
                {
                    profile.BackupAlertList.Add(alertData);
                }
            }));
        }

        public void RemoveAlert(BackupProfileData profile, BackupPerfectAlertTypeEnum alert)
        {
            var foundAlert = profile.BackupAlertList.FirstOrDefault(e => e.AlertType == alert);
            if (foundAlert != null)
            {
                profile.BackupAlertList.Remove(foundAlert);
            }
        }
    }
}
