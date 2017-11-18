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
        BackupItemListEmpty,
        RestoreItemListEmpty,
        BackupItemListFolderNotAvailable,
        BackupItemListFileNotAvailable,

        BackupDestinationFolderNotConfigured,
        RestoreDestinationFolderNotConfigured,
        BackupDestinationFolderNotAvailable,
        RestoreDestinationFolderNotAvailable,

        BackupDestinationNotFound, //folder available no file

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

        public static Dictionary<BackupPerfectAlertValueEnum, BackupPerfectAlertData> BackupPerfectAlertValueDictionary { get; } = new Dictionary<BackupPerfectAlertValueEnum, BackupPerfectAlertData>()
        {
            {
                BackupPerfectAlertValueEnum.BackupItemListEmpty,
                new BackupPerfectAlertData()
                {
                    Name = $"Back up item list is empty",
                    Description = "You must select atleast one backup item, item can be a file or a folder, if the selected item is folder the entire folder content will be backup includeing all sub items. " +
                                "To select items select in the \"Items to backup\" from Setting or press on \"Select items to backup button\""
                }
            },

            {
                BackupPerfectAlertValueEnum.BackupItemListFolderNotAvailable,
                new BackupPerfectAlertData()
                {
                    Name = $"Backup directory is not available",
                }
            },

            {
                BackupPerfectAlertValueEnum.BackupItemListFileNotAvailable,
                new BackupPerfectAlertData()
                {
                    Name = $"Backup file is not available",
                }
            },

            {
                BackupPerfectAlertValueEnum.BackupDestinationFolderNotConfigured,
                new BackupPerfectAlertData()
                {
                    Name = $"Destination backup directory is not set",
                    Description = "Destination backp directory is the destination directory where the backup files will be stored. Please select a destination back directory, it is recomended to select an empty directory",
                }
            },

            {
                BackupPerfectAlertValueEnum.BackupDestinationFolderNotAvailable,
                new BackupPerfectAlertData()
                {
                    Name = $"Destination backup directory is not available",
                    Description = "Destination backp directory is the destination directory where the backup files will be stored, it is recomended to select an empty directory",
                }
            },

            {
                BackupPerfectAlertValueEnum.BackupDestinationNotFound,
                new BackupPerfectAlertData()
                {
                    Name = $"Backup not found, nothing to restore",
                    Description = "Could not find the destination backup.  This usually happens if the destination backup storage is not available or has not been running" ,
                }
            },

            {
                BackupPerfectAlertValueEnum.RestoreItemListEmpty,
                new BackupPerfectAlertData()
                {
                    Name = $"Restore item list is empty",
                    Description = "Please select at least one item to restore",
                }
            },

            {
                BackupPerfectAlertValueEnum.RestoreDestinationFolderNotAvailable,
                new BackupPerfectAlertData()
                {
                    Name = $"Restore directory is not available",
                    Description = "Please select a valid destination directory to store the restored files",
                }
            },

            {
                BackupPerfectAlertValueEnum.RestoreDestinationFolderNotConfigured,
                new BackupPerfectAlertData()
                {
                    Name = $"Restore directory is not configured",
                    Description = "Please select a destination directory to store the restored files",
                }
            },


        };


        public void AddAlert(BackupProfileData profile, BackupPerfectAlertValueEnum alert, string text = null)
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

        //private void AddRestoreAlert(BackupProfileData profile, BackupPerfectAlertData alert)
        //{
        //    Application.Current.Dispatcher.Invoke(new Action(() =>
        //    {
        //        profile.RestoreAlertList.Add(alert);
        //    }));
        //}
    }
}
