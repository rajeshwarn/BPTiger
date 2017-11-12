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

            bBusy = CurrentBackupWorkerTask?.GetProfile() == profile;

            return bBusy;
        }

        public bool? IsBackupWorkerPending(BackupProfileData profile)
        {
            bool? bBusy = null;

            lock (this)
            {
                var found = m_BackupWorkerTaskQueue.FirstOrDefault(w => w.GetProfile() == profile);
                if (found != null)
                {
                    bBusy = true;
                }
            }

            return bBusy;
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

        public void StopBackupTask(BackupProfileData profile)
        {
            if (CurrentBackupWorkerTask.IsPaused == true)
            {
                CurrentBackupWorkerTask.IsPaused = false;
            }

            CurrentBackupWorkerTask.CancelAsync();
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


        List<BackupWorkerTask> m_BackupWorkerTaskQueue = new List<BackupWorkerTask>();

        public void CancelPendingBackupTask(BackupProfileData profile)
        {
            lock (this)
            {
                var task = m_BackupWorkerTaskQueue.FirstOrDefault(w => w.GetProfile() == profile);
                //Since this is an uncommon opertation and the queue is very small we will rebuild the queue
                if (task != null)
                {
                    m_BackupWorkerTaskQueue.Remove(task);
                    profile.IsBackupWorkerPending = true;
                }
            }
        }


        public void CompleteAndStartNextBackup()
        {
            lock (this)
            {
                BackupProjectRepository.Instance.SelectedBackupProject.Logger.Writeln($"<-- Backup completed {CurrentBackupWorkerTask?.GetProfile()?.Name}");
                var task = m_BackupWorkerTaskQueue.FirstOrDefault();
                if (task != null)
                {
                    BackupProjectRepository.Instance.SelectedBackupProject.Logger.Writeln($"--> Starting new backup {task.GetProfile().Name}");
                    CurrentBackupWorkerTask = task;
                    CurrentBackupWorkerTask.RunWorkerAsync();
                }
                else
                {
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
                        m_BackupWorkerTaskQueue.Add(new BackupWorkerTask(profile, bFullBackupScan));
                        profile.IsBackupWorkerPending = true;
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
    }
}
