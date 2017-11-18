using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using CompleteBackup.Models.Profile;
using CompleteBackup.Models.Utilities;
using CompleteBackup.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models.Backup
{
    public class FileSystemProfileBackupWatcherTimer : System.Timers.Timer
    {
        public BackupProfileData Profile;
    }

    public class FileSystemWatcherWorkerTask : BackgroundWorker
    {
        private FileSystemWatcherWorkerTask() { }

        BackupProfileData m_Profile;
        BackupPerfectLogger m_Logger;

        FileSystemProfileBackupWatcherTimer m_FileSystemWatcherBackupTimer;

        public void UpdateFileSystemWatcherInterval(long wakeupIntervalSec)
        {
            if (m_FileSystemWatcherBackupTimer != null)
            {
                m_FileSystemWatcherBackupTimer.Interval = wakeupIntervalSec;
            }
        }

        public static FileSystemWatcherWorkerTask StartNewInstance(BackupProfileData profile, long wakeupIntervalSec)
        {
            var task = new FileSystemWatcherWorkerTask(profile, wakeupIntervalSec);
            task.RunWorkerAsync();

            return task;
        }

        FileSystemWatcherWorkerTask(BackupProfileData profile, long wakeupIntervalSec)
        {
            m_Profile = profile;
            m_Logger = profile.Logger;

            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;

            DoWork += (sender, e) =>
            {
                //var watchList = profile.BackupWatcherItemList;
                try
                {
                    //watchList.Clear();

                    foreach (var backupItem in profile.BackupFolderList)
                    {
                        m_Logger.Writeln($"Starting File System Watcher: {backupItem.Path}");
                        try
                        {
                            new FileSystemWatcerItemManager(m_Profile).RunWatcher(backupItem.Path);
                        }
                        catch (FileNotFoundException)
                        {
                            m_Logger.Writeln($"***Warning: Backup item not available: {backupItem.Path}");
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    m_Logger.Writeln($"File System Watcher exception: {ex.Message}");
                    Trace.WriteLine($"File System Watcher exception: {ex.Message}");
                    e.Result = $"Full Backup exception: {ex.Message}";
                    throw (ex);
                }
                finally
                {
                }
            };

            m_FileSystemWatcherBackupTimer = new FileSystemProfileBackupWatcherTimer()
            {
                Profile = profile,
                Interval = wakeupIntervalSec,
                AutoReset = true,
                Enabled = true,
            };

            m_FileSystemWatcherBackupTimer.Elapsed += BackupTaskManager.OnFileSystemWatcherBackupTimerStartBackup;
        }
    }
}
