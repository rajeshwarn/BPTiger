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
    public class SnapshotBackup : BackupManager
    {
        public SnapshotBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar) { }

        public override void ProcessBackup()
        {
            m_BackupSessionHistory.Reset(GetTimeStamp());

            string backupName = GetTargetSetName();
            string targetSetPath = CreateNewBackupSetFolder(backupName);

            ProcessBackupRootFolders(targetSetPath);

            BackupSessionHistory.SaveHistory(m_TargetBackupPath, backupName, m_BackupSessionHistory);
        }

        protected string CreateNewBackupSetFolder(string newSetName)
        {
            var targetSetPath = m_IStorage.Combine(m_TargetBackupPath, newSetName);
            CreateDirectory(targetSetPath);

            return targetSetPath;
        }

        protected virtual void ProcessBackupRootFolders(string targetPath)
        {
            ProcessNewBackupRootFolders(targetPath);
        }

        protected void ProcessNewBackupRootFolders(string targetPath)
        {
            foreach (var item in m_SourceBackupPathList)
            {
                if (item.IsFolder)
                {
                    var targetFolder = m_IStorage.Combine(targetPath, m_IStorage.GetFileName(item.Path));
                    ProcessSnapshotBackupFolderStep(item.Path, targetFolder);
                }
                else
                {
                    ProcessSnapshotBackupFile(m_IStorage.GetFileName(item.Path), m_IStorage.GetDirectoryName(item.Path), targetPath);
                }
            }
        }


        protected void ProcessSnapshotBackupFile(string file, string sourcePath, string destPath)
        {
            UpdateProgress("Running... ", ++ProcessFileCount, file);

            var fileName = m_IStorage.GetFileName(file);
            // first set, copy to new set
            var sourceFilePath = m_IStorage.Combine(sourcePath, fileName);
            var targetFilePath = m_IStorage.Combine(destPath, fileName);

            CopyFile(sourceFilePath, targetFilePath);

            m_BackupSessionHistory.AddNewFile(sourceFilePath, targetFilePath);
        }

        protected void ProcessSnapshotBackupFolderStep(string sourcePath, string currSetPath)
        {
            var sourceFileList = m_IStorage.GetFiles(sourcePath);
            CreateDirectory(currSetPath);

            foreach (var file in sourceFileList)
            {
                ProcessSnapshotBackupFile(m_IStorage.GetFileName(file), sourcePath, currSetPath);
            }

            //Process directories
            var sourceSubdirectoryEntriesList = GetDirectoriesNames(sourcePath);

            foreach (string subdirectory in sourceSubdirectoryEntriesList)
            {
                string newSourceSetPath = m_IStorage.Combine(sourcePath, subdirectory);
                string newCurrSetPath = m_IStorage.Combine(currSetPath, subdirectory);

                ProcessSnapshotBackupFolderStep(newSourceSetPath, newCurrSetPath);
            }
        }
    }
}
