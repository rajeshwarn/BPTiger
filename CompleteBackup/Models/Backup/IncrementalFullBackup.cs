using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.History;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Storage;
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
    public class IncrementalFullBackup : FullBackup
    {
        public IncrementalFullBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar)
        {
            m_IStorage = new FileSystemStorage();
        }

        public override void ProcessBackup()
        {
            m_BackupSessionHistory.Reset(GetTimeStamp());
            string backupName = GetTargetSetName();

            var targetSetPath = BackupManager.GetLastBackupSetName(m_Profile);
            if (targetSetPath == null)
            {
                //this is the first run
                targetSetPath = CreateNewBackupSetFolder(backupName);
            }

            ProcessBackupRootFolders(targetSetPath);

            BackupSessionHistory.SaveHistory(m_TargetBackupPath, backupName, m_BackupSessionHistory);
        }
    }
}
