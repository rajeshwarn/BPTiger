using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup;
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
    public class IncrementalFullWatcherBackup : FileSystemWatcherBackup
    {
        public IncrementalFullWatcherBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar) { }

        public override void ProcessBackup()
        {
            m_BackupSessionHistory.Reset(GetTimeStamp());

            var backupName = BackupBase.GetLastBackupSetName(m_Profile);
            if (backupName == null)
            {
                //First backup
                backupName = GetTargetSetName();
                ProcessNewBackupRootFolders(CreateNewBackupSetFolder(backupName));
            }
            else
            {
                ProcessBackupRootFolders(m_IStorage.Combine(m_TargetBackupPath, backupName));
            }

            BackupSessionHistory.SaveHistory(m_TargetBackupPath, backupName, m_BackupSessionHistory);
        }
    }
}
