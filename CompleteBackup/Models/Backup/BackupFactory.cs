using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models.backup
{
    class BackupFactory
    {
        public static BackgroundWorker CreateFullBackupTaskWithProgressBar(BackupProfileData profile)
        {
            var progressBar = GenericStatusBarView.NewInstance;
            progressBar.UpdateProgressBar("Backup starting...", 0);

            return CreateFullBackupTask(profile, progressBar);
        }
        public static BackgroundWorker CreateFullBackupTask(BackupProfileData profile, GenericStatusBarView progressBar = null)
        {
            var task = new BackgroundWorker();

            task.WorkerReportsProgress = true;
            task.WorkerSupportsCancellation = true;
            task.DoWork += (sender, e) =>
            {
                profile.IsBackupRunning = true;
                try
                {

                    //                    BackupManager backup = new IncrementalBackup(sourcePath, currSetPath, new NullStorage(), progressBar);
                    BackupManager backup = new IncrementalBackup(profile.FolderList.ToList(), profile.TargetBackupFolder, new FileSystemStorage(), progressBar);
                    //                    BackupManager backup = new IncrementalBackup(sourcePath, currSetPath, new FileSystemStorage(), progressBar);
                    //                    BackupManager backup = new OneWaySyncBackup("CBKP-Snap_2017-10-13_12501247655", sourcePath, currSetPath, new FileSystemStorage(), progressBar);

                    backup.Init();
                    backup.ProcessBackup();
                    backup.Done();
                }
                catch (TaskCanceledException ex)
                {
                    progressBar.UpdateProgressBar("Completed with Errors");
                    progressBar.Release();
                    Trace.WriteLine($"Full Backup exception: {ex.Message}");
                    e.Result = $"Full Backup exception: {ex.Message}";
                    throw (ex);
                }
                finally
                {
                    profile.IsBackupRunning = false;
                }
            };

            return task;
        }
    }
}
