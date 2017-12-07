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
    public class SnapshotBackup : BackupBase
    {
        public SnapshotBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar) { }

        public override void ProcessBackup()
        {
            m_BackupSessionHistory.Reset(GetTimeStamp(), GetTargetSetName(), m_SourceBackupPathList, m_TargetBackupPath);

            var backupPath = GetTargetBackupPathWithSetPath(m_TargetBackupPath);
            CreateDirectory(backupPath);

            ProcessBackupRootFolders(backupPath);

            m_BackupSessionHistory.SaveHistory();
        }

        protected string CreateNewBackupSetFolder(string newSetName)
        {
            var targetSetPath = m_IStorage.Combine(m_TargetBackupPath, newSetName);

            CreateDirectory(targetSetPath);

            return targetSetPath;
        }

        protected virtual void ProcessBackupRootFolders(string targetPath, string lastTargetPath = null)
        {
            ProcessNewBackupRootFolders(targetPath);
        }

        protected void ProcessNewBackupRootFolders(string targetPath)
        {
            foreach (var item in m_SourceBackupPathList)
            {
                if (item.IsFolder)
                {
                    if (m_IStorage.DirectoryExists(item.Path))
                    {
                        item.IsAvailable = true;
                        var targetFolder = m_IStorage.Combine(targetPath, m_IStorage.GetFileName(item.Path));
                        ProcessSnapshotBackupFolderStep(item.Path, targetFolder);
                    }
                    else
                    {
                        item.IsAvailable = false;
                        m_Logger.Writeln($"***Warning: Skipping unavailable backup folder: {item.Path}");
                    }                
                }
                else
                {
                    ProcessSnapshotBackupFile(item.Path, m_IStorage.GetDirectoryName(item.Path), targetPath);
                }
            }
        }


        protected void ProcessSnapshotBackupFile(string file, string sourcePath, string destPath)
        {
            if (CheckCancellationPendingOrSleep()) { return; }

            UpdateProgress("Running... ", ++ProcessFileCount, file);

            try
            {
                var fileName = m_IStorage.GetFileName(file);
                // first set, copy to new set
                var sourceFilePath = m_IStorage.Combine(sourcePath, fileName);
                var targetFilePath = m_IStorage.Combine(destPath, fileName);

                CopyFile(sourceFilePath, targetFilePath);

                m_BackupSessionHistory.AddNewFile(sourceFilePath, targetFilePath);
            }
            catch (Exception ex)
            {
                m_Logger.Writeln($"**Exception while processing file: {sourcePath}, dest path: {destPath}\n{ex.Message}");
            }
        }

        protected void ProcessSnapshotBackupFolderStep(string sourcePath, string currSetPath)
        {
            if (CheckCancellationPendingOrSleep()) { return; }

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

                try
                {
                    ProcessSnapshotBackupFolderStep(newSourceSetPath, newCurrSetPath);
                }
                catch (Exception ex)
                {
                    m_Logger.Writeln($"**Exception while processing folder: {sourcePath}, dest path: {currSetPath}\n{ex.Message}");
                }
            }
        }
    }
}
