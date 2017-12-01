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
using System.Windows;

namespace CompleteBackup.Models.Profile
{
    internal struct LASTINPUTINFO
    {
        public int cbSize;

        public int dwTime;
    }

    public class BackupProcessWorkerTask : BackgroundWorker
    {
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);


        BackupBase m_BackupManager = null;

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
        bool m_bFullBackupScan;

        int m_MaxActiveCPU;
        int m_MaxIdleCPU;

        public BackupProfileData GetProfile()
        {
            return m_Profile;
        }

        public void SleepIfNeeded()
        {
            return;


            //PerformanceCounter("Processor", "% Processor Time", "_Total");
            //PerformanceCounter("Processor", "% Privileged Time", "_Total");
            //PerformanceCounter("Processor", "% Interrupt Time", "_Total");
            //PerformanceCounter("Processor", "% DPC Time", "_Total");
            //PerformanceCounter("Memory", "Available MBytes", null);
            //PerformanceCounter("Memory", "Committed Bytes", null);
            //PerformanceCounter("Memory", "Commit Limit", null);
            //PerformanceCounter("Memory", "% Committed Bytes In Use", null);
            //PerformanceCounter("Memory", "Pool Paged Bytes", null);
            //PerformanceCounter("Memory", "Pool Nonpaged Bytes", null);
            //PerformanceCounter("Memory", "Cache Bytes", null);
            //PerformanceCounter("Paging File", "% Usage", "_Total");
            //PerformanceCounter("PhysicalDisk", "Avg. Disk Queue Length", "_Total");
            //PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
            //PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
            //PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Read", "_Total");
            //PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Write", "_Total");
            //PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
            //PerformanceCounter("Process", "Handle Count", "_Total");
            //PerformanceCounter("Process", "Thread Count", "_Total");
            //PerformanceCounter("System", "Context Switches/sec", null);
            //PerformanceCounter("System", "System Calls/sec", null);
            //PerformanceCounter("System", "Processor Queue Length", null);


            PerformanceCounter cpuCounter  = new PerformanceCounter("Processor", "% Processor Time", "_Total");
  //          PerformanceCounter currCPUTime = new PerformanceCounter("Processor", "% Processor Time", null);//, Process.GetCurrentProcess().ProcessName);
            
            //PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            //PerformanceCounter diskCounter = new PerformanceCounter("FileSystem Disk Activity", "FileSystem Bytes Written", "_Total");
            PerformanceCounter diskTime = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");

            //PerformanceCounter diskCounter2 = new PerformanceCounter("PhysicalDisk", "% Idle Time", "_Total");


            dynamic firstValue = cpuCounter.NextValue();
            dynamic secondValue = cpuCounter.NextValue();

//            dynamic currCPUVal1 = currCPUTime.NextValue();
//            dynamic currCPUVal2 = currCPUTime.NextValue();


            float tmp = diskTime.NextValue();
            var DISKTime = (float)(Math.Round((double)tmp, 1));

            //dynamic fsValue1 = diskCounter.NextValue();
            //dynamic fsValue2 = diskCounter.NextValue();
            //dynamic fsValue21 = diskCounter.NextValue();
            //dynamic fsValue22 = diskCounter.NextValue();

            //dynamic fsValue3 = diskCounter2.NextValue();
            //dynamic fsValue4 = diskCounter2.NextValue();
            //dynamic fsValue41 = diskCounter2.NextValue();
            //dynamic fsValue42 = diskCounter2.NextValue();

            //Int32 diskUsage = Convert.ToInt32(fsValue2);




        //LASTINPUTINFO lastinputinfo = new LASTINPUTINFO();
        //    lastinputinfo.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(lastinputinfo);
        //    GetLastInputInfo(ref lastinputinfo);
        //    var res =  (((Environment.TickCount & int.MaxValue) - (lastinputinfo.dwTime & int.MaxValue)) & int.MaxValue);

        //    var iIdel = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;

            if (secondValue >= m_MaxActiveCPU)
            {
                if (DISKTime >= m_MaxActiveCPU)
                {

                    //Thread.Sleep(500);
                }
            }

            return;
        }

        private BackupProcessWorkerTask() { }

        private void BackupTaskRunWorkerProgressChangedEvent(object sender, ProgressChangedEventArgs e)
        {

        }

        private void BackupTaskRunWorkerCompletedEvent(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                m_Logger.Writeln($"Backup task canceled");
            }
            else if (e.Error != null)
            {
                m_Logger.Writeln($"***Backup task ended with error:\n{e.Error.Message}");
            }
            else
            {
                m_Logger.Writeln($"Backup task ended normally");
            }

            lock (this)
            {

            }

            BackupTaskManager.Instance.CompleteAndStartNextBackup(this);
        }

        public BackupProcessWorkerTask(BackupProfileData profile, bool bFullBackupScan)
        {
            m_Profile = profile;
            m_Logger = profile.Logger;
            m_bFullBackupScan = bFullBackupScan;

            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;

            DoWork += (sender, e) =>
            {
                m_MaxActiveCPU = Properties.General.Default.MaxCPUOnBusy;
                m_MaxIdleCPU = Properties.General.Default.MaxCPUOnAway;

                m_ProgressBar = GenericStatusBarView.NewInstance;
                profile.IsBackupWorkerBusy = true;
                //                RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackupTaskRunWorkerCompletedEvent);
                ProgressChanged += BackupTaskRunWorkerProgressChangedEvent;
                RunWorkerCompleted += BackupTaskRunWorkerCompletedEvent;

                //profile.IsBackupWorkerBusy = true;
                m_ProgressBar.UpdateProgressBar("Backup starting...", 0);
                var startTime = DateTime.Now;

                m_Logger.Writeln($"-------------------------------------------------");
                try
                {
                    switch (profile.BackupType)
                    {
                        case BackupTypeEnum.Differential:
                            if (bFullBackupScan)
                            {
                                m_Logger.Writeln($"Starting a Full Deep Differential backup");
                                m_BackupManager = new DifferentialBackup(profile, m_ProgressBar);
                            }
                            else
                            {
                                m_Logger.Writeln($"Starting a Differential watcher backup");
                                m_BackupManager = new DifferentialWatcherBackup(profile, m_ProgressBar);
                            }
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
                            throw new ArgumentNullException("Unknown backup type in BackupWorker");
                    }

                    m_BackupManager.Init();
                    m_BackupManager.ProcessBackup();
                    m_BackupManager.Done();

                    m_Profile.RefreshProfileProperties();

                }
                catch (TaskCanceledException ex)
                {
                    m_Logger.Writeln($"Backup exception: {ex.Message}");
                    m_ProgressBar.UpdateProgressBar("Completed with Errors");
                    Trace.WriteLine($"Full Backup exception: {ex.Message}");
                    e.Result = $"Full Backup exception: {ex.Message}";
                    throw (ex);
                }
                finally
                {
                    if (CancellationPending)
                    {
                        e.Cancel = true;
                    }

                    m_Logger.Writeln($"Backup completed, execution time: {DateTime.Now - startTime}");
                    m_ProgressBar.Release();
                    m_ProgressBar = null;
                    m_BackupManager = null;
                }
            };
        }
    }
}
