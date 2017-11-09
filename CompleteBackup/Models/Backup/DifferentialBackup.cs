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
    public class DifferentialBackup : SnapshotBackup
    {
        public string LastSetPath;

        public DifferentialBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar) { }

        public override void ProcessBackup()
        {
            m_BackupSessionHistory.Reset(GetTimeStamp());

            var targetSet = GetTargetSetName();
            var lastSet = BackupManager.GetLastBackupSetName(m_Profile);
            if (lastSet == null)
            {
                //First backup
                base.ProcessBackupRootFolders(CreateNewBackupSetFolder(targetSet));
            }
            else
            {

                var lastTargetPath_ = m_IStorage.Combine(m_TargetBackupPath, lastSet);
                var newTargetPath = m_IStorage.Combine(m_TargetBackupPath, targetSet);

                if (!MoveDirectory(lastTargetPath_, newTargetPath))
                {
                    m_Logger.Writeln($"***Backup failed, failed to move {lastTargetPath_} To {newTargetPath}");
                    MessageBox.Show($"Operation Canceled", "Incremental Backup", MessageBoxButton.OK, MessageBoxImage.Information);

                    return;
                }

                CreateDirectory(lastTargetPath_);

                //move old history files
                var fileEntries = m_IStorage.GetFiles(newTargetPath);
                foreach (string fileName in fileEntries.Where(f => BackupSessionHistory.IsHistoryFile(f)))
                {
                    MoveFile(m_IStorage.Combine(newTargetPath, m_IStorage.GetFileName(fileName)),
                             m_IStorage.Combine(lastTargetPath_, m_IStorage.GetFileName(fileName)));
                }


                ProcessDifferentialBackupRootFolders(newTargetPath, lastTargetPath_);

                ////check if set was changed and need to be deleted
                //var prevSetList = m_IStorage.GetDirectories(newTargetPath);
                //foreach (var path in prevSetList)
                //{
                //    var setName = m_IStorage.GetFileName(path);
                //    var foundMatch = m_SourceBackupPathList.Where(f => m_IStorage.GetFileName(f.Path) == setName);
                //    if (foundMatch.Count() == 0)
                //    {
                //        var sourcePath = m_IStorage.Combine(m_IStorage.GetDirectoryName(m_SourceBackupPathList.FirstOrDefault().Path), setName);

                //        var targetPath = m_IStorage.Combine(newTargetPath, setName);
                //        var lastTargetPath = m_IStorage.Combine(lastTargetPath_, setName);
                //        MoveDirectory(targetPath, lastTargetPath);

                //        m_BackupSessionHistory.AddDeletedFolder(sourcePath, newTargetPath);
                //    }
                //}

                //foreach (var item in m_SourceBackupPathList)
                //{
                //    var targetdirectoryName = m_IStorage.GetFileName(item.Path);

                //    var targetPath = m_IStorage.Combine(newTargetPath, targetdirectoryName);
                //    var lastTargetPath = m_IStorage.Combine(lastTargetPath_, targetdirectoryName);

                //    if (item.IsFolder)
                //    {
                //        ProcessIncrementalFolderStep(item.Path, targetPath, lastTargetPath);
                //    }
                //    else
                //    {
                //        UpdateProgress("Running... ", ++ProcessFileCount, item.Path);
                //        HandleFile(m_IStorage.GetDirectoryName(item.Path), newTargetPath, lastTargetPath, targetdirectoryName);
                //    }
                //}

                BackupSessionHistory.SaveHistory(m_TargetBackupPath, targetSet, m_BackupSessionHistory);
            }
        }

        protected virtual void ProcessDifferentialBackupRootFolders(string newTargetPath, string lastTargetPath_)
        {
            var sourceDirectoryEntriesList = m_SourceBackupPathList.Where(i => i.IsFolder).ToList();
            var sourceFileEntriesList = m_SourceBackupPathList.Where(i => !i.IsFolder).ToList();

            //process all items
            foreach (var item in m_SourceBackupPathList)
            {
                var targetdirectoryName = m_IStorage.GetFileName(item.Path);

                var targetPath = m_IStorage.Combine(newTargetPath, targetdirectoryName);
                var lastTargetPath = m_IStorage.Combine(lastTargetPath_, targetdirectoryName);

                if (item.IsFolder)
                {
                    if (m_IStorage.DirectoryExists(item.Path))
                    {
                        item.IsAvailable = true;
                        ProcessDifferentialBackupFolderStep(item.Path, targetPath, lastTargetPath);
                    }
                    else
                    {
                        item.IsAvailable = false;
                        m_Logger.Writeln($"***Warning: Skipping unavailable backup folder: {item.Path}");
                    }
                }
                else
                {
                    UpdateProgress("Running... ", ++ProcessFileCount, item.Path);
                    ProcessDeferentialBackupFile(m_IStorage.GetDirectoryName(item.Path), newTargetPath, lastTargetPath_, targetdirectoryName);
                }
            }

            //var sourceDirList = new List<FolderData>();

            //Handle deleted folders
            //foreach (var item in sourceDirectoryEntriesList)
            //{
            //    sourceDirList.Add(item);// m_IStorage.GetFileName(item.Path));
            //}
            HandleDeletedItems(sourceDirectoryEntriesList, newTargetPath, lastTargetPath_);

            //sourceDirList.Clear();

            //Handle deleted files
            //foreach (var item in sourceFileEntriesList)
            //{
            //    sourceDirList.Add(item);// m_IStorage.GetFileName(item.Path));
            //}
            HandleDeletedFiles(sourceFileEntriesList, newTargetPath, lastTargetPath_);
        }

        protected void HandleDeletedItems(object sourceSubdirectoryEntriesList, string currSetPath, string lastSetPath)
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
                    var destPath = m_IStorage.Combine(lastSetPath, m_IStorage.GetFileName(entry));
                    m_IStorage.MoveDirectory(entry, destPath, true);

                    m_BackupSessionHistory.AddDeletedFolder(entry, destPath);
                }
            }
        }

        protected void HandleDeletedFiles(object sourceFileList, string currSetPath, string lastSetPath)
        {
            var folderDataList = sourceFileList as List<FolderData>;
            var itemStringList = sourceFileList as List<string>;

            //Delete any deleted files
            var currSetFileList = m_IStorage.GetFiles(currSetPath);
            foreach (var filePath in currSetFileList)
            {
                var fileName = m_IStorage.GetFileName(filePath);
                if ((folderDataList != null && !folderDataList.Exists(item => m_IStorage.GetFileName(item.Path) == fileName)) ||
                    (itemStringList != null && !itemStringList.Exists(item => m_IStorage.GetFileName(item) == fileName)))
                {
                    //if not exists in source, delete the file
                    var prevSetfilePath = m_IStorage.Combine(lastSetPath, fileName);

                    //Move file to last set
                    MoveFile(filePath, prevSetfilePath, true);

                    m_BackupSessionHistory.AddDeletedFile(filePath, prevSetfilePath);
                }
            }
        }

        protected void ProcessDeferentialBackupFile(string sourcePath, string currSetPath, string lastSetPath, string fileName)
        {
            var sourceFilePath = m_IStorage.Combine(sourcePath, fileName);
            var lastSetFilePath = (lastSetPath == null) ? null : m_IStorage.Combine(lastSetPath, fileName);
            var currSetFilePath = m_IStorage.Combine(currSetPath, fileName);

            if (m_IStorage.FileExists(currSetFilePath))
            {
                if (m_IStorage.IsFileSame(sourceFilePath, currSetFilePath))
                {
                    //File is the same, do nothing
                    m_BackupSessionHistory.AddNoChangeFile(sourceFilePath, currSetFilePath);
                }
                else
                {
                    //Move current file to old set
                    //if (!m_IStorage.DirectoryExists(lastSetPath))
                    //{
                    //    m_IStorage.CreateDirectory(lastSetPath);
                    //}

                    //Keep current version in set
                    MoveFile(currSetFilePath, lastSetFilePath, true);

                    //Update new version to new set
                    if (!m_IStorage.DirectoryExists(currSetPath))
                    {
                        CreateDirectory(currSetPath);
                    }
                    CopyFile(sourceFilePath, currSetFilePath);

                    m_BackupSessionHistory.AddUpdatedFile(sourceFilePath, currSetFilePath);
                }
            }
            else
            {
                // new file, copy to current set
                if (!m_IStorage.DirectoryExists(currSetPath))
                {
                    CreateDirectory(currSetPath);
                }
                CopyFile(sourceFilePath, currSetFilePath);

                m_BackupSessionHistory.AddNewFile(sourceFilePath, currSetFilePath);
            }
        }

        public void ProcessDifferentialBackupFolderStep(string sourcePath, string currSetPath, string lastSetPath)
        {
            var sourceFileList = m_IStorage.GetFiles(sourcePath);

            m_IStorage.CreateDirectory(currSetPath, true);

            foreach (var file in sourceFileList)
            {
                var fileName = m_IStorage.GetFileName(file);
                UpdateProgress("Running... ", ++ProcessFileCount, file);

                ProcessDeferentialBackupFile(sourcePath, currSetPath, lastSetPath, fileName);
            }

            HandleDeletedFiles(sourceFileList, currSetPath, lastSetPath);

            //Process directories
            var sourceSubdirectoryEntriesList = GetDirectoriesNames(sourcePath);

            HandleDeletedItems(sourceSubdirectoryEntriesList, currSetPath, lastSetPath);

            foreach (string subdirectory in sourceSubdirectoryEntriesList)
            {
                string newSourceSetPath = m_IStorage.Combine(sourcePath, subdirectory);
                string newCurrSetPath = m_IStorage.Combine(currSetPath, subdirectory);
                string newLastSetPath = m_IStorage.Combine(lastSetPath, subdirectory);

                ProcessDifferentialBackupFolderStep(newSourceSetPath, newCurrSetPath, newLastSetPath);
            }
        }
    }
}
