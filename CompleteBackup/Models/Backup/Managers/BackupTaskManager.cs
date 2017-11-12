using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CompleteBackup.Models.Profile
{
    class BackupTaskManager
    {
        static public BackupTaskManager Instance { get; set; } = new BackupTaskManager();

        List<BackupWorkerTask> m_BackupWorkerTaskQueue = new List<BackupWorkerTask>();
        //public BackupWorkerTask CurrentBackupWorkerTask { get; set; }

        [MethodImpl(MethodImplOptions.Synchronized)]
        BackupWorkerTask GetRunningBackupWorkerTask(BackupProfileData profile)
        {
            BackupWorkerTask task = null;

            //lock (this)
            {
                task = m_BackupWorkerTaskQueue.FirstOrDefault(w => (w.GetProfile() == profile) && (w.IsBusy));
            }

            return task;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        BackupWorkerTask GetPendingBackupWorkerTask(BackupProfileData profile)
        {
            BackupWorkerTask task = null;

           // lock (this)
            {
                task = m_BackupWorkerTaskQueue.FirstOrDefault(w => (w.GetProfile() == profile) && !w.IsBusy);
            }

            return task;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        BackupWorkerTask GetPausedBackupWorkerTask(BackupProfileData profile)
        {
            BackupWorkerTask task = null;

            // lock (this)
            {
                task = m_BackupWorkerTaskQueue.FirstOrDefault(w => (w.GetProfile() == profile) && w.IsBusy && (w.IsPaused == true));
            }

            return task;
        }

        public bool? IsBackupWorkerBusy(BackupProfileData profile)
        {
            bool? bBusy = null;

            bBusy = GetRunningBackupWorkerTask(profile) != null;

            return bBusy;
        }


        public bool? IsBackupWorkerPending(BackupProfileData profile)
        {
            bool? bPending = null;

            bPending = GetPendingBackupWorkerTask(profile) != null;

            return bPending;
        }

        public bool? IsBackupWorkerPaused(BackupProfileData profile)
        {
            bool? bPaused = null;

            var task = GetPausedBackupWorkerTask(profile);
            bPaused = (task != null);

            return bPaused;
        }


        public void PauseBackup(BackupProfileData profile)
        {
            var task = GetRunningBackupWorkerTask(profile);
            if (task != null && task.IsPaused == false)
            {
                task.IsPaused = true;
            }
        }

        public void ResumeBackup(BackupProfileData profile)
        {
            var task = GetRunningBackupWorkerTask(profile);
            if (task != null && task.IsPaused == true)
            {
                task.IsPaused = false;
            }
        }

        public void StopBackupTask(BackupProfileData profile)
        {
            lock (this)
            {
                var task = GetRunningBackupWorkerTask(profile);
                if (task != null)
                {
                    if (task.IsPaused == true)
                    {
                        task.IsPaused = false;
                    }
                }

                task.CancelAsync();
            }
        }

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


        public void CompleteAndStartNextBackup(BackupWorkerTask completedTask)
        {
            BackupProjectRepository.Instance.SelectedBackupProject.Logger.Writeln($"<-- Backup completed {completedTask.GetProfile().Name}");

            lock (this)
            {
                m_BackupWorkerTaskQueue.Remove(completedTask);
            }
            StartNextBackupTask();
        }


        void StartNextBackupTask()
        {
            BackupWorkerTask task = null;
            lock (this)
            {
                task = m_BackupWorkerTaskQueue.Where(t => t.IsBusy == false).FirstOrDefault();
                task?.RunWorkerAsync();
            }

            if (task != null)
            {
                BackupProjectRepository.Instance.SelectedBackupProject.Logger.Writeln($"--> Starting new backup {task.GetProfile().Name}");
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

        public int MaxConcurrentTasks = 3;

        public void StartBackup(BackupProfileData profile, bool bFullBackupScan)
        {
            if (profile.TargetBackupFolder == null)
            {
                return;
            }

            lock (this)
            {
                m_BackupWorkerTaskQueue.Add(new BackupWorkerTask(profile, bFullBackupScan));

                StartNextBackupTask();

                //if (CurrentBackupWorkerTask == null)
                //{
                //    BackupProjectRepository.Instance.SelectedBackupProject.Logger.Writeln($"--> Starting new backup {profile.Name}");
                //    CurrentBackupWorkerTask = new BackupWorkerTask(profile, bFullBackupScan);
                //    CurrentBackupWorkerTask.RunWorkerAsync();
                //}
                //else
                //{
                //    //check if there is a task pending...
                //    var pendingTask = m_BackupWorkerTaskQueue.FirstOrDefault(t => t.GetProfile() == profile);
                //    if (pendingTask == null)
                //    {
                //        m_BackupWorkerTaskQueue.Add(new BackupWorkerTask(profile, bFullBackupScan));
                //        profile.IsBackupWorkerPending = true;
                //    }
                //}
            }
        }
    }
}
