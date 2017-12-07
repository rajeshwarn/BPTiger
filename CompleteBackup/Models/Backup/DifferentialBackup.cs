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
    public class DifferentialBackup : IncrementalFullBackup
    {
        public string LastSetPath;

        public DifferentialBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar) { }

        public override void ProcessBackup()
        {
            m_BackupSessionHistory.Reset(GetTimeStamp(), GetTargetSetName(), m_SourceBackupPathList, m_TargetBackupPath);

            var lastSetName = BackupBase.GetLastBackupSetName_(m_Profile);
            if (lastSetName == null)
            {
                //First backup
                var backupPath = GetTargetBackupPathWithSetPath(m_TargetBackupPath);
                CreateDirectory(backupPath);

                ProcessNewBackupRootFolders(backupPath);
            }
            else
            {
                var lastSetPath = m_IStorage.Combine(m_TargetBackupPath, lastSetName);
                var targetSetPath = GetTargetBackupPathWithSetName(m_TargetBackupPath);

                CreateNewBackupSetFolderAndMoveDataToOldSet(targetSetPath, lastSetPath);

                var lastSetArchivePath = GetTargetArchivePath(lastSetPath);
                var targetSetArchivePath = GetTargetBackupPathWithSetPath(m_TargetBackupPath);

                ProcessBackupRootFolders(targetSetArchivePath, lastSetArchivePath);

                var sourceDirectoryEntriesList = m_SourceBackupPathList.Where(i => i.IsFolder).ToList();
                var sourceFileEntriesList = m_SourceBackupPathList.Where(i => !i.IsFolder).ToList();
                HandleDeletedItems(sourceDirectoryEntriesList, targetSetArchivePath, lastSetArchivePath);
                HandleDeletedFiles(sourceFileEntriesList, targetSetArchivePath, lastSetArchivePath);
            }

            m_BackupSessionHistory.SaveHistory();
        }

        protected override void ProcessBackupRootFolders(string newTargetPath, string lastTargetPath)
        {
            //process all items
            foreach (var item in m_SourceBackupPathList)
            {
                var targetdirectoryName = m_IStorage.GetFileName(item.Path);

                if (item.IsFolder)
                {
                    if (m_IStorage.DirectoryExists(item.Path))
                    {
                        item.IsAvailable = true;
                        var newTargetPathDir = m_IStorage.Combine(newTargetPath, targetdirectoryName);
                        var lastTargetPathDir = m_IStorage.Combine(lastTargetPath, targetdirectoryName);
                        ProcessDifferentialBackupFolderStep(item.Path, newTargetPathDir, lastTargetPathDir);
                    }
                    else
                    {
                        item.IsAvailable = false;
                        m_Logger.Writeln($"***Warning: Skipping unavailable backup folder: {item.Path}");
                    }
                }
                else
                {
                    ProcessDeferentialBackupFile(item.Path, newTargetPath, lastTargetPath, targetdirectoryName);
                }            
            }
        }



        protected void ProcessDeferentialBackupFile(string sourcePath, string currSetPath, string lastSetPath, string fileName)
        {
            if (CheckCancellationPendingOrSleep()) { return; }

            UpdateProgress("Running... ", ++ProcessFileCount, fileName);

            var lastSetFilePath = (lastSetPath == null) ? null : m_IStorage.Combine(lastSetPath, fileName);
            var currSetFilePath = m_IStorage.Combine(currSetPath, fileName);

            try
            {
                if (m_IStorage.FileExists(currSetFilePath))
                {
                    if (m_IStorage.IsFileSame(sourcePath, currSetFilePath))
                    {
                        //File is the same, do nothing
                        HandleSameFile(sourcePath, currSetFilePath);
                    }
                    else
                    {
                        //Keep current version in set
                        MoveFile(currSetFilePath, lastSetFilePath, true);

                        //Update new version to new set
                        if (!m_IStorage.DirectoryExists(currSetPath))
                        {
                            CreateDirectory(currSetPath);
                        }

                        CopyFile(sourcePath, currSetFilePath);

                        m_BackupSessionHistory.AddUpdatedFile(sourcePath, lastSetFilePath);
                    }
                }
                else
                {
                    // new file, copy to current set
                    if (!m_IStorage.DirectoryExists(currSetPath))
                    {
                        CreateDirectory(currSetPath);
                    }
                    CopyFile(sourcePath, currSetFilePath);

                    m_BackupSessionHistory.AddNewFile(sourcePath, currSetFilePath);
                }
            }
            catch (Exception ex)
            {
                m_Logger.Writeln($"**Exception while procesing file set\nSource: {sourcePath}\nTarget: {currSetFilePath}\nLast: {lastSetFilePath}\n{ex.Message}");
            }
        }

        public void ProcessDifferentialBackupFolderStep(string sourcePath, string currSetPath, string lastSetPath)
        {
            var sourceFileList = m_IStorage.GetFiles(sourcePath);

            m_IStorage.CreateDirectory(currSetPath, true);

            foreach (var file in sourceFileList)
            {
                var fileName = m_IStorage.GetFileName(file);
                ProcessDeferentialBackupFile(file, currSetPath, lastSetPath, fileName);
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

                try
                {
                    ProcessDifferentialBackupFolderStep(newSourceSetPath, newCurrSetPath, newLastSetPath);
                }
                catch (Exception ex)
                {
                    m_Logger.Writeln($"**Exception while procesing folder set\nSource: {newSourceSetPath}\nTarget: {newCurrSetPath}\nLast: {newLastSetPath}\n{ex.Message}");
                }
            }
        }
    }
}
