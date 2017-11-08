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
    public class IncrementalFullBackup : SnapshotBackup
    {
        public IncrementalFullBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar) { }

        public override void ProcessBackup()
        {
            m_BackupSessionHistory.Reset(GetTimeStamp());
            var backupName = BackupManager.GetLastBackupSetName(m_Profile);
            if (backupName == null)
            {
                //First backup
                backupName = GetTargetSetName();
                ProcessBackupRootFolders(CreateNewBackupSetFolder(backupName));
            }
            else
            {
                ProcessBackupRootFolders(m_IStorage.Combine(m_TargetBackupPath, backupName));
            }

            BackupSessionHistory.SaveHistory(m_TargetBackupPath, backupName, m_BackupSessionHistory);//, GetTimeStampString());
        }


        protected override void ProcessBackupRootFolders(string targetPath)
        {
            //process all items
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

            var sourceDirList = new List<string>();

            //Handle deleted folders
            var sourceDirectoryEntriesList = m_SourceBackupPathList.Where(i => i.IsFolder).ToList();
            foreach (var item in sourceDirectoryEntriesList)
            {
                sourceDirList.Add(m_IStorage.GetFileName(item.Path));
            }
            HandleDeletedItems(sourceDirList, targetPath);

            sourceDirList.Clear();

            //Handle deleted files
            var sourceFileEntriesList = m_SourceBackupPathList.Where(i => !i.IsFolder).ToList();
            foreach (var item in sourceFileEntriesList)
            {
                sourceDirList.Add(m_IStorage.GetFileName(item.Path));
            }
            HandleDeletedFiles(sourceDirList, targetPath);
        }


        protected override void ProcessSnapshotBackupFile(string fileName, string sourcePath, string destPath)
        {
            var sourceFilePath = m_IStorage.Combine(sourcePath, fileName);
            var currSetFilePath = m_IStorage.Combine(destPath, fileName);

            UpdateProgress("Running... ", ++ProcessFileCount, sourceFilePath);

            if (m_IStorage.FileExists(currSetFilePath))
            {
                if (m_IStorage.IsFileSame(sourceFilePath, currSetFilePath))
                {
                    //Do nothing
                    m_BackupSessionHistory.AddNoChangeFile(sourceFilePath, currSetFilePath);
                }
                else
                {
                    //update/overwrite file
                    CopyFile(sourceFilePath, currSetFilePath, true);

                    m_BackupSessionHistory.AddUpdatedFile(sourceFilePath, currSetFilePath);
                }
            }
            else
            {
                if (!m_IStorage.DirectoryExists(destPath))
                {
                    CreateDirectory(destPath);
                }
                CopyFile(sourceFilePath, currSetFilePath);

                m_BackupSessionHistory.AddNewFile(sourceFilePath, currSetFilePath);
            }
        }

        protected override void ProcessSnapshotBackupFolderStep(string sourcePath, string currSetPath)
        {
            var sourceFileList = m_IStorage.GetFiles(sourcePath);

            if (!m_IStorage.DirectoryExists(currSetPath))
            {
                CreateDirectory(currSetPath);
            }

            foreach (var file in sourceFileList)
            {
                ProcessSnapshotBackupFile(m_IStorage.GetFileName(file), sourcePath, currSetPath);
            }

            HandleDeletedFiles(sourceFileList, currSetPath);

            //Process directories
            var sourceSubdirectoryEntriesList = GetDirectoriesNames(sourcePath);

            foreach (string subdirectory in sourceSubdirectoryEntriesList)
            {
                string newSourceSetPath = m_IStorage.Combine(sourcePath, subdirectory);
                string newCurrSetPath = m_IStorage.Combine(currSetPath, subdirectory);

                ProcessSnapshotBackupFolderStep(newSourceSetPath, newCurrSetPath);
            }

            HandleDeletedItems(sourceSubdirectoryEntriesList, currSetPath);
        }

    }
}
