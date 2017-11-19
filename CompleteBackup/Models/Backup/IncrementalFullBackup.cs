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
using System.Threading;
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

            var newFullTargetPath = m_IStorage.Combine(m_TargetBackupPath, backupName);

            var sourceDirectoryEntriesList = m_SourceBackupPathList.Where(i => i.IsFolder).ToList();
            var sourceFileEntriesList = m_SourceBackupPathList.Where(i => !i.IsFolder).ToList();
            HandleDeletedItems(sourceDirectoryEntriesList, newFullTargetPath);
            HandleDeletedFiles(sourceFileEntriesList, newFullTargetPath);

            BackupSessionHistory.SaveHistory(m_TargetBackupPath, backupName, m_BackupSessionHistory);
        }


        protected override void ProcessBackupRootFolders(string targetPath, string lastTargetPath = null)
        {
            //process all items
            foreach (var item in m_SourceBackupPathList)
            {
                if (item.IsFolder)
                {
                    if (m_IStorage.DirectoryExists(item.Path))
                    {
                        item.IsAvailable = true;
                        var targetFolder = m_IStorage.Combine(targetPath, m_IStorage.GetFileName(item.Path));

                        ProcessIncrementalBackupFolderStep(item.Path, targetFolder);                            
                    }
                    else
                    {
                        item.IsAvailable = false;
                        m_Logger.Writeln($"***Warning: Skipping unavailable backup folder: {item.Path}");
                    }
                }
                else
                {
                    ProcessIncrementalBackupFile(m_IStorage.GetFileName(item.Path), m_IStorage.GetDirectoryName(item.Path), targetPath);
                }
            }
        }


        protected void ProcessIncrementalBackupFile(string fileName, string sourcePath, string destPath)
        {
            if (CheckCancellationPending()) { return; }

            var sourceFilePath = m_IStorage.Combine(sourcePath, fileName);
            var currSetFilePath = m_IStorage.Combine(destPath, fileName);

            UpdateProgress("Running... ", ++ProcessFileCount, fileName);

            try
            {
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
            catch (Exception ex)
            {
                m_Logger.Writeln($"**Exception while procesing file: {sourcePath}, target: {destPath}\n{ex.Message}");
            }
        }

        protected void ProcessIncrementalBackupFolderStep(string sourcePath, string currSetPath, string lastSetPath = null)
        {
            var sourceFileList = m_IStorage.GetFiles(sourcePath);

            if (!m_IStorage.DirectoryExists(currSetPath))
            {
                CreateDirectory(currSetPath);
            }

            foreach (var file in sourceFileList)
            {
                ProcessIncrementalBackupFile(m_IStorage.GetFileName(file), sourcePath, currSetPath);
            }

            HandleDeletedFiles(sourceFileList, currSetPath);

            //Process directories
            var sourceSubdirectoryEntriesList = GetDirectoriesNames(sourcePath);

            foreach (string subdirectory in sourceSubdirectoryEntriesList)
            {
                string newSourceSetPath = m_IStorage.Combine(sourcePath, subdirectory);
                string newCurrSetPath = m_IStorage.Combine(currSetPath, subdirectory);

                try
                {
                    ProcessIncrementalBackupFolderStep(newSourceSetPath, newCurrSetPath);
                }
                catch (Exception ex)
                {
                    m_Logger.Writeln($"**Exception while procesing directory: {newSourceSetPath}, target: {newCurrSetPath}\n{ex.Message}");
                }
            }

            HandleDeletedItems(sourceSubdirectoryEntriesList, currSetPath);
        }


        protected void HandleDeletedItems(object sourceSubdirectoryEntriesList, string currSetPath, string lastSetPath = null)
        {
            var folderDataList = sourceSubdirectoryEntriesList as List<FolderData>;
            var itemStringList = sourceSubdirectoryEntriesList as List<string>;

            //lookup for deleted items
            if (m_IStorage.DirectoryExists(currSetPath))
            {
                string[] targetSubdirectoryEntries = m_IStorage.GetDirectories(currSetPath);
                var deleteList = new List<string>();
                if (targetSubdirectoryEntries != null)
                {
                    foreach (var entry in targetSubdirectoryEntries)
                    {
                        if (((folderDataList != null) && !folderDataList.Exists(e => !e.IsAvailable || (m_IStorage.GetFileName(e.Path) == m_IStorage.GetFileName(entry)))) ||
                            ((itemStringList != null) && !itemStringList.Exists(e => m_IStorage.GetFileName(e) == m_IStorage.GetFileName(entry))))
                        {
                            deleteList.Add(entry);
                        }
                    }
                }

                //Delete deleted items
                foreach (var entry in deleteList)
                {
                    try
                    {
                        if (lastSetPath != null)
                        {
                            var destPath = m_IStorage.Combine(lastSetPath, m_IStorage.GetFileName(entry));
                            MoveDirectory(entry, destPath, true);
                        }
                        else
                        {
                            DeleteDirectory(m_IStorage.Combine(currSetPath, entry));
                        }
                    }
                    catch (Exception ex)
                    {
                        m_Logger.Writeln($"**Exception while deleting directory: {entry}\n{ex.Message}");
                    }

                    m_BackupSessionHistory.AddDeletedFolder(entry, currSetPath);
                }
            }
        }

        protected void HandleDeletedFiles(object sourceFileList, string currSetPath, string lastSetPath = null)
        {
            var folderDataList = sourceFileList as List<FolderData>;
            var itemStringList = sourceFileList as List<string>;

            //Delete any deleted files
            var currSetFileList = m_IStorage.GetFiles(currSetPath);
            foreach (var filePath in currSetFileList)
            {
                try
                {
                    var fileName = m_IStorage.GetFileName(filePath);
                    if ((folderDataList != null && !folderDataList.Exists(item => m_IStorage.GetFileName(item.Path) == fileName)) ||
                        (itemStringList != null && !itemStringList.Exists(item => m_IStorage.GetFileName(item) == fileName)))
                    {
                        //if not exists in source, delete the file
                        if (lastSetPath != null)
                        {
                            var prevSetfilePath = m_IStorage.Combine(lastSetPath, fileName);

                            //Move file to last set
                            MoveFile(filePath, prevSetfilePath, true);
                            m_BackupSessionHistory.AddDeletedFile(filePath, prevSetfilePath);
                        }
                        else
                        {
                            DeleteFile(filePath);
                            m_BackupSessionHistory.AddDeletedFile(filePath, filePath);
                        }

                    }
                }
                catch (Exception ex)
                {
                    m_Logger.Writeln($"**Exception while deleting file: {filePath}\n{ex.Message}");
                }
            }
        }

        protected void CreateNewBackupSetFolderAndMoveDataToOldSet(string newFullTargetPath, string lastFullTargetPath)
        {
            //Rename last set to new set name
            if (!MoveDirectory(lastFullTargetPath, newFullTargetPath))
            {
                m_Logger.Writeln($"***Backup failed, failed to move {lastFullTargetPath} To {newFullTargetPath}");
                MessageBox.Show($"Operation Canceled", "Incremental Backup", MessageBoxButton.OK, MessageBoxImage.Information);

                throw new Exception($"Failed to move backup set {lastFullTargetPath} to {newFullTargetPath}");
            }

            //Create last set folder - initially empty, will copy changed item from newFullTargetPath
            CreateDirectory(lastFullTargetPath);

            //Move history file to last set
            var fileEntries = m_IStorage.GetFiles(newFullTargetPath);
            foreach (string fileName in fileEntries.Where(f => BackupSessionHistory.IsHistoryFile(f)))
            {
                MoveFile(m_IStorage.Combine(newFullTargetPath, m_IStorage.GetFileName(fileName)),
                         m_IStorage.Combine(lastFullTargetPath, m_IStorage.GetFileName(fileName)));
            }
        }
    }
}
