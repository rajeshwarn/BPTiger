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
    public class FileSystemWatcerItemManager
    {
        BackupProfileData m_Profile;
        BackupPerfectLogger m_Logger;
        string m_Path;

        FileSystemWatcerItemManager() { }

        public FileSystemWatcerItemManager(BackupProfileData profile)
        {
            m_Profile = profile;
            m_Logger = profile.Logger;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void RunWatcher(string path)
        {
            m_Path = path;

            var watcher = new FileSystemWatcher();
            if (m_Profile.GetStorageInterface().IsFolder(m_Path))
            {
                watcher.IncludeSubdirectories = true;
                watcher.Path = m_Path;
                watcher.NotifyFilter =
                    NotifyFilters.LastAccess |
                    NotifyFilters.LastWrite |
                    NotifyFilters.FileName |
                    NotifyFilters.DirectoryName |
                    NotifyFilters.CreationTime;

                watcher.Filter = "*.*";

                // Add event handlers.
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnCreated);
                watcher.Deleted += new FileSystemEventHandler(OnDeleted);
                watcher.Renamed += new RenamedEventHandler(OnRenamed);
            }
            else
            {
                watcher.Path = m_Profile.GetStorageInterface().GetDirectoryName(m_Path);
                watcher.NotifyFilter =
                    NotifyFilters.LastAccess |
                    NotifyFilters.LastWrite |
                    NotifyFilters.FileName;

                watcher.Filter = $"{m_Profile.GetStorageInterface().GetFileName(m_Path)}";

                // Add event handlers.
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Deleted += new FileSystemEventHandler(OnDeleted);
                watcher.Renamed += new RenamedEventHandler(OnRenamed);
            }

            watcher.InternalBufferSize = 6400000;

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        // Define the event handlers.
        private void OnCreated(object source, FileSystemEventArgs e)
        {
            m_Profile.AddItemToBackupWatcherItemList(new FileSystemWatcherItemData { WatchPath = m_Path, Time = DateTime.Now, ChangeType = e.ChangeType, FullPath = e.FullPath, Name = e.Name });
//            BackupProjectRepository.Instance.SaveProject();
//            m_Logger.Writeln($"Watcher, File {e.ChangeType}: {e.FullPath }");
        }

        //private int onChangedFireCount = 0;
        private void OnChanged(object source, FileSystemEventArgs e)
        {
//            onChangedFireCount++;
//            if (onChangedFireCount == 1)
            {
//                if (!m_Profile.GetStorageInterface().IsFolder(e.FullPath))
                {
                    m_Profile.AddItemToBackupWatcherItemList(new FileSystemWatcherItemData { WatchPath = m_Path, Time = DateTime.Now, ChangeType = e.ChangeType, FullPath = e.FullPath, Name = e.Name });
//                    BackupProjectRepository.Instance.SaveProject();
 //                   m_Logger.Writeln($"Watcher, File {e.ChangeType}: {e.FullPath }");
                }
            }
            //else
            //{
            //    onChangedFireCount = 0;
            //}
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            m_Profile.AddItemToBackupWatcherItemList(new FileSystemWatcherItemData { WatchPath = m_Path, Time = DateTime.Now, ChangeType = e.ChangeType, FullPath = e.FullPath, Name = e.Name });
//            BackupProjectRepository.Instance.SaveProject();
//            m_Logger.Writeln($"Watcher, File {e.ChangeType}: {e.FullPath }");
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            m_Profile.AddItemToBackupWatcherItemList(new FileSystemWatcherItemData { WatchPath = m_Path, Time = DateTime.Now, ChangeType = e.ChangeType, OldPath = e.OldFullPath, FullPath = e.FullPath, Name = e.Name });
 //           BackupProjectRepository.Instance.SaveProject();
//            m_Logger.Writeln($"Watcher, File Renamed: {e.OldFullPath} to {e.FullPath}");
        }
    }

}
