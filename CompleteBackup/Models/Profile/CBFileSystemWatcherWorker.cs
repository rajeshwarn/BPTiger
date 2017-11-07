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
    public class WatcherItemData
    {
        public DateTime Time { get; set; }
        public WatcherChangeTypes ChangeType { get; set; }
        public string OldPath { get; set; }
        public string FullPath { get; set; }
        public string Name { get; set; }

    }


    public class CBFileSystemWatcherWorker : BackgroundWorker
    {
        private CBFileSystemWatcherWorker() { }

        BackupProfileData m_Profile;
        BackupPerfectLogger m_Logger;

        public CBFileSystemWatcherWorker(BackupProfileData profile)
        {
            m_Profile = profile;
            m_Logger = profile.Logger;

            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;

            DoWork += (sender, e) =>
            {
                var watchList = profile.BackupWatcherItemList;

                m_Logger.Writeln($"Starting File System Watcher, profile: {profile.Name}");

                try
                {
                    watchList.Clear();

                    foreach (var backupItem in profile.BackupFolderList)
                    {
                        RunTask(backupItem.Path);
                    }

                    //                    RunTask(@"E:\test2\source");
                }
                catch (TaskCanceledException ex)
                {
                    Trace.WriteLine($"Full Backup exception: {ex.Message}");
                    e.Result = $"Full Backup exception: {ex.Message}";
                    throw (ex);
                }
                finally
                {
                }
            };
        }
    

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected void RunTask(string path)
        {
            //string[] args = System.Environment.GetCommandLineArgs();

            //// If a directory is not specified, exit program.
            //if (args.Length != 2)
            //{
            //    // Display the proper way to call the program.
            //    Console.WriteLine("Usage: Watcher.exe (directory)");
            //    return;
            //}

            // Create a new FileSystemWatcher and set its properties.
            var watcher = new FileSystemWatcher() { IncludeSubdirectories = true };
            watcher.Path = path;// args[1];
            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter = "*.*";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.Deleted += new FileSystemEventHandler(OnDeleted);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;

            // Wait for the user to quit the program.
            Console.WriteLine("Press \'q\' to quit the sample.");
          //  while (Console.Read() != 'q') ;
        }

        // Define the event handlers.
        private void OnCreated(object source, FileSystemEventArgs e)
        {
            m_Profile.BackupWatcherItemList.Add(new WatcherItemData { Time = DateTime.Now, ChangeType = e.ChangeType, FullPath = e.FullPath, Name = e.Name });
            BackupProjectRepository.Instance.SaveProject();
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            m_Logger.Writeln($"Watcher, File {e.ChangeType}: {e.FullPath }");
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            m_Profile.BackupWatcherItemList.Add(new WatcherItemData { Time = DateTime.Now, ChangeType = e.ChangeType, FullPath = e.FullPath, Name = e.Name });
            BackupProjectRepository.Instance.SaveProject();
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            m_Logger.Writeln($"Watcher, File {e.ChangeType}: {e.FullPath }");
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            m_Profile.BackupWatcherItemList.Add(new WatcherItemData { Time = DateTime.Now, ChangeType = e.ChangeType, FullPath = e.FullPath, Name = e.Name });
            BackupProjectRepository.Instance.SaveProject();
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            m_Logger.Writeln($"Watcher, File {e.ChangeType}: {e.FullPath }");
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            m_Profile.BackupWatcherItemList.Add(new WatcherItemData { Time = DateTime.Now, ChangeType = e.ChangeType, OldPath = e.OldFullPath, FullPath = e.FullPath, Name = e.Name });
            BackupProjectRepository.Instance.SaveProject();
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
            m_Logger.Writeln($"Watcher, File renamed: {e.OldFullPath} to {e.FullPath}");
        }
    }
}
