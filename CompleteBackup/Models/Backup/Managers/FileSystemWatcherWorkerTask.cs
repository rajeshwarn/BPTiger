using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
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
    public class FileSystemWatcherWorkerTask : BackgroundWorker
    {
        private FileSystemWatcherWorkerTask() { }

        BackupProfileData m_Profile;
        BackupPerfectLogger m_Logger;




        public FileSystemWatcherWorkerTask(BackupProfileData profile)
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
        }
    

    }
}
