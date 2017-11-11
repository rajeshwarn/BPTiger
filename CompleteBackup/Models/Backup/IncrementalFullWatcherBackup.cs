using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.History;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Views;
using CompleteBackup.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CompleteBackup.Models.backup
{
    public class IncrementalFullWatcherBackup : IncrementalFullBackup
    {
        public IncrementalFullWatcherBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar) { }

        protected override void ProcessBackupRootFolders(string targetPath, string lastTargetPath = null)
        {
            ProcessBackupWatcherRootFolders(targetPath);
            //try
            //{
            //    ProcessBackupWatcherRootFolders(targetPath);
            //}
            //catch (Exception ex)
            //{
            //    m_Logger.Writeln($"***IncrementalFullWatcherBackup Exception\n{ex.Message}");
            //}
        }
    }
}
