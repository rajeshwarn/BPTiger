using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Storage;
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
        private BackupWorker() { }

        public BackupWorker(BackupProfileData profile)
        {
            m_Profile = profile;
            m_ProgressBar = GenericStatusBarView.NewInstance;

            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;

            DoWork += (sender, e) =>
            {
                profile.IsBackupWorkerBusy = true;
                m_ProgressBar.UpdateProgressBar("Backup starting...", 0);
                try
                {
                    switch (profile.BackupType)
                    {
                        case BackupTypeEnum.Incremental:
                            m_BackupManager = new IncrementalBackup(profile, m_ProgressBar);

                            break;

                        case BackupTypeEnum.Full:
                        default:
                            m_BackupManager = new OneWaySyncBackup(profile, m_ProgressBar);
                            break;
                    }

                    //                    m_BackupManager = new IncrementalBackup(sourcePath, currSetPath, new NullStorage(), progressBar);
                    //m_BackupManager = new IncrementalBackup(profile.FolderList.ToList(), profile.TargetBackupFolder, new FileSystemStorage(), m_ProgressBar);
                    //                    m_BackupManager = new IncrementalBackup(sourcePath, currSetPath, new FileSystemStorage(), progressBar);
                    //                    m_BackupManager = new OneWaySyncBackup("CBKP-Snap_2017-10-13_12501247655", sourcePath, currSetPath, new FileSystemStorage(), progressBar);

                    m_BackupManager.Init();
                    m_BackupManager.ProcessBackup();
                    m_BackupManager.Done();
                }
                catch (TaskCanceledException ex)
                {
                    m_ProgressBar.UpdateProgressBar("Completed with Errors");
                    m_ProgressBar.Release();
                    Trace.WriteLine($"Full Backup exception: {ex.Message}");
                    e.Result = $"Full Backup exception: {ex.Message}";
                    throw (ex);
                }
                finally
                {
                    m_BackupManager = null;
                    profile.IsBackupWorkerBusy = false;
                }
            };
        }
    }
}
