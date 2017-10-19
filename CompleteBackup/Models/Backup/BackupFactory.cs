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
        public static BackgroundWorker CreateFullBackupTask(List<string> sourcePath, string currSetPath, GenericStatusBarView progressBar = null)
        {
            var task = new BackgroundWorker();

            task.WorkerReportsProgress = true;
            task.WorkerSupportsCancellation = true;
            task.DoWork += (sender, e) =>
            {
                //var collection = e.Argument as EventCollectionDataBase;
                try
                {

//                    BackupSet backup = new IncrementalBackup(sourcePath, currSetPath, new NullStorage(), progressBar);
                    BackupSet backup = new IncrementalBackup(sourcePath, currSetPath, new FileSystemStorage(), progressBar);
                    //                    BackupSet backup = new IncrementalBackup(sourcePath, currSetPath, new FileSystemStorage(), progressBar);
                    //                    BackupSet backup = new OneWaySyncBackup("CBKP-Snap_2017-10-13_12501247655", sourcePath, currSetPath, new FileSystemStorage(), progressBar);

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
            };

            return task;
        }
    }
}
