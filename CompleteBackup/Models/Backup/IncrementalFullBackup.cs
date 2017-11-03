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

            var newTargetPath = m_IStorage.Combine(TargetPath, backupName);
            m_IStorage.CreateDirectory(newTargetPath);

            foreach (var item in SourcePath)
            {
                if (item.IsFolder)
                {
                    var targetPath = m_IStorage.Combine(newTargetPath, m_IStorage.GetFileName(item.Path));
                    ProcessFullBackupFolderStep(item.Path, targetPath);
                }
                else
                {
                    ProcessFullBackupFile(item.Path, m_IStorage.GetDirectoryName(item.Path), newTargetPath);
                }
            }

            BackupSessionHistory.SaveHistory(TargetPath, backupName, m_BackupSessionHistory);
        }
    }
}
