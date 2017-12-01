using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup;
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
    public class DifferentialWatcherBackup : FileSystemWatcherBackup
    {
        public DifferentialWatcherBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar) { }


        public override void ProcessBackup()
        {
            m_BackupSessionHistory.Reset(GetTimeStamp(), m_TargetBackupPath);

            var targetSetName = GetTargetSetName();
            var lastSetName = GetLastBackupSetName_(m_Profile);
            if (lastSetName == null)
            {
                //First backup
                ProcessNewBackupRootFolders(CreateNewBackupSetFolder(targetSetName));
            }
            else
            {
                var lastFullTargetPath = m_IStorage.Combine(m_TargetBackupPath, GetLastBackupSetPath_(m_Profile));
                var newFullTargetPath = m_IStorage.Combine(m_TargetBackupPath, GetTargetSetNamePath());

                CreateNewBackupSetFolderAndMoveDataToOldSet(newFullTargetPath, lastFullTargetPath);

                ProcessBackupRootFolders(newFullTargetPath, lastFullTargetPath);
            }

            BackupSessionHistory.SaveHistory(m_TargetBackupPath, targetSetName, m_BackupSessionHistory);
        }
    }
}
