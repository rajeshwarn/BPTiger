using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CompleteBackup.Models.Profile
{
    class BackupTaskManager
    {
        static public BackupTaskManager Instance { get; set; } = new BackupTaskManager();

        public BackupWorkerTask CurrentBackupWorkerTask { get; set; }

        public bool? IsBackupWorkerBusy(BackupProfileData profile)
        {
            bool? bBusy = null;

            var profileList = BackupProjectRepository.Instance.SelectedBackupProject.BackupProfileList;

            if (profileList.Contains(profile))
            {
                bBusy = CurrentBackupWorkerTask != null && CurrentBackupWorkerTask.IsBusy;
            }

            return bBusy;
        }


        public void PauseBackupTask(BackupProfileData profile, bool bPause)
        {
            CurrentBackupWorkerTask.IsPaused = bPause;
        }

        public bool? IsBackupWorkerPaused(BackupProfileData profile)
        {
            bool? bPaused = null;

            var profileList = BackupProjectRepository.Instance.SelectedBackupProject.BackupProfileList;

            if (profileList.Contains(profile))
            {
                bPaused = CurrentBackupWorkerTask != null && CurrentBackupWorkerTask.IsPaused == true;
            }

            return bPaused;
        }


        Queue<BackupWorkerTask> m_BackupWorkerTaskQueue = new Queue<BackupWorkerTask>();


        static int tastCount = 0;
        public void CompleteAndStartNextBackup()
        {
            lock (this)
            {
                BackupProjectRepository.Instance.SelectedBackupProject.Logger.Writeln($"<-- Backup completed {CurrentBackupWorkerTask?.GetProfile()?.Name}");
                if (m_BackupWorkerTaskQueue.Count > 0)
                {
                    var task = m_BackupWorkerTaskQueue.Dequeue();
                    BackupProjectRepository.Instance.SelectedBackupProject.Logger.Writeln($"--> Starting new backup {task.GetProfile().Name}");
                    CurrentBackupWorkerTask = task;
                    CurrentBackupWorkerTask.RunWorkerAsync();
                    if (tastCount > 1)
                    {
                        int i = 0;
                    }
                }
                else
                {
                    tastCount--;
                    CurrentBackupWorkerTask = null;
                }
            }
        }


        public static void OnFileSystemWatcherBackupTimerStartBackup(Object source, ElapsedEventArgs e)
        {
            var profile = ((FileSystemProfileBackupWatcherTimer)source).Profile;
            if (profile?.BackupWatcherItemList.Count() > 0)
            {
                profile.Logger.Writeln($"OnFileSystemWatcherBackupTimer - start backup");
                BackupTaskManager.Instance.StartBackup(profile, false);
            }
        }

        public void StartBackup(BackupProfileData profile, bool bFullBackupScan)
        {
            lock (this)
            {
                if (profile.TargetBackupFolder == null)
                {
                    return;
                }

                if (CurrentBackupWorkerTask == null)
                {
                    tastCount++;

                    if (tastCount > 1)
                    {
                        int i = 0;
                    }

                    BackupProjectRepository.Instance.SelectedBackupProject.Logger.Writeln($"--> Starting new backup {profile.Name}");
                    CurrentBackupWorkerTask = new BackupWorkerTask(profile, bFullBackupScan);
                    CurrentBackupWorkerTask.RunWorkerAsync();
                }
                else
                {
                    //check if there is a task pending...
                    var pendingTask = m_BackupWorkerTaskQueue.FirstOrDefault(t => t.GetProfile() == profile);
                    if (pendingTask == null)
                    {
                        m_BackupWorkerTaskQueue.Enqueue(new BackupWorkerTask(profile, bFullBackupScan));
                    }
                }

                //if (CurrentBackupWorkerTask != null && CurrentBackupWorkerTask.IsBusy)
                //{
                //    profile.Logger.Writeln($"***Start failed, backup is already running");
                //}
                //else
                //{
                //    CurrentBackupWorkerTask = new BackupWorkerTask(profile, bFullBackupScan);
                //    CurrentBackupWorkerTask.RunWorkerAsync();
                //}
            }
        }

        public void PauseBackup(BackupProfileData profile)
        {
            if (CurrentBackupWorkerTask != null && CurrentBackupWorkerTask.IsBusy)
            {
                CurrentBackupWorkerTask.IsPaused = true;
            }
        }
        public void ResumeBackup(BackupProfileData profile)
        {
            if (CurrentBackupWorkerTask != null && CurrentBackupWorkerTask.IsBusy)
            {
                CurrentBackupWorkerTask.IsPaused = false;
            }
        }
    }
}
