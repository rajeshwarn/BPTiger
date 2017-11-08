using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Models.Utilities;
using CompleteBackup.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CompleteBackup.Models.Profile
{
    public class BackupWorker : BackgroundWorker
    {
        BackupManager m_BackupManager = null;

        public bool? IsPaused { get {
                bool? bpause = !m_BackupManager?.PauseWaitHandle.WaitOne(0);
                return !m_BackupManager?.PauseWaitHandle.WaitOne(0); }
            set {
                if (value == true) {
                    m_BackupManager?.PauseWaitHandle.Reset(); }
                else {
                    m_BackupManager?.PauseWaitHandle.Set(); }
            } }


        GenericStatusBarView m_ProgressBar;
        BackupProfileData m_Profile;
        protected BackupPerfectLogger m_Logger;

        private BackupWorker() { }

        public BackupWorker(BackupProfileData profile, bool bFullBackupScan)
        {
            m_Profile = profile;
            m_ProgressBar = GenericStatusBarView.NewInstance;
            m_Logger = profile.Logger;

            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;

            DoWork += (sender, e) =>
            {
                profile.IsBackupWorkerBusy = true;
                m_ProgressBar.UpdateProgressBar("Backup starting...", 0);
                var startTime = DateTime.Now;
                m_Logger.Writeln($"-------------------------------------------------");
                try
                {
                    switch (profile.BackupType)
                    {
                        case BackupTypeEnum.Differential:
                            m_Logger.Writeln($"Starting a Full Deep Differential backup");
                            m_BackupManager = new DifferentialBackup(profile, m_ProgressBar);
                            break;

                        case BackupTypeEnum.Incremental:
                            if (bFullBackupScan)
                            {
                                m_Logger.Writeln($"Starting a Full Deep Incremental backup");
                                m_BackupManager = new IncrementalFullBackup(profile, m_ProgressBar);
                            }
                            else
                            {
                                m_Logger.Writeln($"Starting Incremental watcher backup");
                                m_BackupManager = new IncrementalFullWatcherBackup(profile, m_ProgressBar);
                            }
                            break;                            

                        case BackupTypeEnum.Snapshot:
                            m_Logger.Writeln($"Starting a Snapshot backup");
                            m_BackupManager = new SnapshotBackup(profile, m_ProgressBar);
                            break;

                        default:
                            throw new ArgumentNullException("Unknown backup type in BAckupWorker");
                    }

                    m_BackupManager.Init();
                    m_BackupManager.ProcessBackup();
                    m_BackupManager.Done();

                    m_Profile.UpdateProfileProperties();

                }
                catch (TaskCanceledException ex)
                {
                    m_Logger.Writeln($"Backup exception: {ex.Message}");
                    m_ProgressBar.UpdateProgressBar("Completed with Errors");
                    m_ProgressBar.Release();
                    Trace.WriteLine($"Full Backup exception: {ex.Message}");
                    e.Result = $"Full Backup exception: {ex.Message}";
                    throw (ex);
                }
                finally
                {
                    m_Logger.Writeln($"Backup completed, execution time: {DateTime.Now - startTime}");

                    m_BackupManager = null;
                    profile.IsBackupWorkerBusy = false;
                }
            };
        }
    }
}
