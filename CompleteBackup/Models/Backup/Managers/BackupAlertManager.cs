﻿using CompleteBackup.DataRepository;
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
        public bool IsDeletable { get { return NotificationType == BackupPerfectNotificationTypeEnum.Notification; } }
        public DateTime AlertTime { get; set; }
        public BackupPerfectNotificationTypeEnum NotificationType { get; set; } = BackupPerfectNotificationTypeEnum.Warning;
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageName { get { return BackupAlertManager.BackupPerfectNotificationTypeToImageNameDictionary[NotificationType]; } }
    }

    public enum BackupPerfectNotificationTypeEnum
    {
        Notification,
        Warning,
        Error,
    }
    //public enum BackupPerfectAlertSourceEnum
    //{
    //    Generic,
    //    Backup,
    //    Restore,
    //}

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

        RestoreSessionListIsEmpty,

        Restore_BackupDestinationNotFound, //folder available no file

        BackupInSleepMode,

        BackupFileSystemWatcherNotRunning,

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

        public static Dictionary<BackupPerfectNotificationTypeEnum, string> BackupPerfectNotificationTypeToImageNameDictionary { get; } = new Dictionary<BackupPerfectNotificationTypeEnum, string>()
        {
            { BackupPerfectNotificationTypeEnum.Notification, "/Resources/Icons/Information.ico"},
            { BackupPerfectNotificationTypeEnum.Warning, "/Resources/Icons/Alert.ico"},
            { BackupPerfectNotificationTypeEnum.Error, "/Resources/Icons/Error.ico"},
        };


        public static Dictionary<BackupPerfectAlertTypeEnum, Func<BackupPerfectAlertData>> BackupPerfectAlertValueDictionary { get; } = new Dictionary<BackupPerfectAlertTypeEnum, Func<BackupPerfectAlertData>>()
        {
            {
                BackupPerfectAlertTypeEnum.BackupFileSystemWatcherNotRunning,
                ()=> new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.BackupFileSystemWatcherNotRunning,
                    NotificationType = BackupPerfectNotificationTypeEnum.Notification,
                    Name = $"Realtime File System watched turned off",
                    Description = "Backup is still running but will not monitor realtime changes, backup will start based on the schedule under the profile setting"
                }
            },

            {
                BackupPerfectAlertTypeEnum.BackupInSleepMode,
                ()=> new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.BackupInSleepMode,
                    NotificationType = BackupPerfectNotificationTypeEnum.Notification,
                    Name = $"Back up in sleep mode",
                    Description = "Sleep mode prevents from being backed up, during this mode automatic or scheduled backup will not run, While can wakeup from sleep mode anytime by pressing on the \"Wakeup\" button or wait for the sleep time to expire"
                }
            },

            {
                BackupPerfectAlertTypeEnum.BackupItemListEmpty,
                ()=> new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.BackupItemListEmpty,
                    NotificationType = BackupPerfectNotificationTypeEnum.Warning,
                    Name = $"Back up item list is empty",
                    Description = "You must select atleast one backup item, item can be a file or a folder, if the selected item is folder the entire folder content will be backup includeing all sub items. " +
                                "To select items select in the \"Items to backup\" from Setting or press on \"Select items to backup button\""
                }
            },

            {
                BackupPerfectAlertTypeEnum.BackupItemListFolderNotAvailable,
                ()=> new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.BackupItemListFolderNotAvailable,
                    NotificationType = BackupPerfectNotificationTypeEnum.Warning,
                    Name = $"Backup directory is not available",
                }
            },

            {
                BackupPerfectAlertTypeEnum.BackupItemListFileNotAvailable,
                ()=> new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.BackupItemListFileNotAvailable,
                    NotificationType = BackupPerfectNotificationTypeEnum.Warning,
                    Name = $"Backup file not available",
                }
            },

            {
                BackupPerfectAlertTypeEnum.BackupDestinationFolderNotConfigured,
                ()=> new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.BackupDestinationFolderNotConfigured,
                    NotificationType = BackupPerfectNotificationTypeEnum.Warning,
                    Name = $"Destination backup directory is not set",
                    Description = "Destination backp directory is the destination directory where the backup files will be stored. Please select a destination back directory, it is recomended to select an empty directory",
                }
            },

            {
                BackupPerfectAlertTypeEnum.BackupDestinationFolderNotAvailable,
                ()=> new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.BackupDestinationFolderNotAvailable,
                    NotificationType = BackupPerfectNotificationTypeEnum.Warning,
                    Name = $"Destination backup directory is not available",
                    Description = "Destination backp directory is the destination directory where the backup files will be stored, it is recomended to select an empty directory",
                }
            },

            {
                BackupPerfectAlertTypeEnum.Restore_BackupDestinationNotFound,
                ()=> new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.Restore_BackupDestinationNotFound,
                    NotificationType = BackupPerfectNotificationTypeEnum.Warning,
                    Name = $"Backup not found, nothing to restore",
                    Description = "Could not find the destination backup.  This usually happens if the destination backup storage is not available or has not been running" ,
                }
            },

            {
                BackupPerfectAlertTypeEnum.RestoreItemListEmpty,
                ()=> new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.RestoreItemListEmpty,
                    NotificationType = BackupPerfectNotificationTypeEnum.Warning,
                    Name = $"Restore item list is empty",
                    Description = "Please select at least one item to restore",
                }
            },

            {
                BackupPerfectAlertTypeEnum.RestoreDestinationFolderNotAvailable,
                ()=> new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.RestoreDestinationFolderNotAvailable,
                    NotificationType = BackupPerfectNotificationTypeEnum.Warning,
                    Name = $"Restore directory is not available",
                    Description = "Please select a valid destination directory to store the restored files",
                }
            },

            {
                BackupPerfectAlertTypeEnum.RestoreSessionListIsEmpty,
                ()=> new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.RestoreSessionListIsEmpty,
                    NotificationType = BackupPerfectNotificationTypeEnum.Warning,
                    Name = $"Backup data not found. Did you run backup for the first time?",
                    Description = "No backup data - Did you run backup for the first time?",
                }
            },

            {
                BackupPerfectAlertTypeEnum.RestoreDestinationFolderNotConfigured,
                ()=> new BackupPerfectAlertData()
                {
                    AlertType = BackupPerfectAlertTypeEnum.RestoreDestinationFolderNotConfigured,
                    NotificationType = BackupPerfectNotificationTypeEnum.Warning,
                    Name = $"Restore directory is not configured",
                    Description = "Please select a destination directory to store the restored files",
                }
            },
        };


        public void AddAlert(BackupProfileData profile, BackupPerfectAlertTypeEnum alert, string text = null)
        {
            var alertData = BackupPerfectAlertValueDictionary[alert]();

            alertData.AlertTime = DateTime.Now;

            if (text != null)
            {
                alertData.Name += text;
            }            

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (alert.ToString().StartsWith("Restore"))
                {
                    var foundAlert = profile.RestoreAlertList.FirstOrDefault(e => e.AlertType == alert);
                    if (foundAlert == null)
                    {
                        profile.RestoreAlertList.Add(alertData);
                    }
                }
                else
                {
                    var foundAlert = profile.BackupAlertList.FirstOrDefault(e => e.AlertType == alert);
                    if (foundAlert == null)
                    {
                        profile.BackupAlertList.Add(alertData);
                    }
                }
            }));
        }

        public void RemoveAlert(BackupProfileData profile, BackupPerfectAlertTypeEnum alert)
        {
//            var foundAlert = profile.BackupAlertList.FirstOrDefault(e => e.IsDeletable && e.AlertType == alert);
            var foundAlert = profile.BackupAlertList.FirstOrDefault(e => e.AlertType == alert);
            if (foundAlert != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    profile.BackupAlertList.Remove(foundAlert);
                }));
            }

            foundAlert = profile.RestoreAlertList.FirstOrDefault(e => e.IsDeletable && e.AlertType == alert);
            if (foundAlert != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    profile.RestoreAlertList.Remove(foundAlert);
                }));
            }
        }
    }
}
