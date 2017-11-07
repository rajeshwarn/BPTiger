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
    public class IncrementalFullBackup : FullBackup
    {
        public IncrementalFullBackup(BackupProfileData profile, bool bFullBackupScan, GenericStatusBarView progressBar = null) : base(profile, bFullBackupScan, progressBar)
        {
            m_IStorage = new FileSystemStorage();
        }

        public override void ProcessBackup()
        {
            m_BackupSessionHistory.Reset(GetTimeStamp());
            //path = profile.GetStorageInterface().Combine(profile.TargetBackupFolder, path);
            var backupName = BackupManager.GetLastBackupSetName(m_Profile);
            if (backupName == null)
            {
                //this is the first run
                backupName = GetTargetSetName();
                CreateNewBackupSetFolder(backupName);
            }

            var targetSetPath = m_IStorage.Combine(m_TargetBackupPath, backupName);

            ProcessBackupRootFolders(targetSetPath);

            BackupSessionHistory.SaveHistory(m_TargetBackupPath, backupName, m_BackupSessionHistory);//, GetTimeStampString());
        }


        protected override void ProcessBackupRootFolders(string targetPath)
        {
            if (IsFullBackupScan)
            {
                foreach (var item in m_SourceBackupPathList)
                {
                    if (item.IsFolder)
                    {
                        var targetFolder = m_IStorage.Combine(targetPath, m_IStorage.GetFileName(item.Path));

                        ProcessFullBackupFolderStep(item.Path, targetFolder);
                    }
                    else
                    {
                        ProcessFullBackupFile(m_IStorage.GetFileName(item.Path), m_IStorage.GetDirectoryName(item.Path), targetPath);
                    }
                }


                var sourceDirList = new List<string>();
                var sourceDirectoryEntriesList = m_SourceBackupPathList.Where(i => i.IsFolder).ToList();
                foreach (var item in sourceDirectoryEntriesList)
                {
                    sourceDirList.Add(m_IStorage.GetFileName(item.Path));
                }
                HandleDeletedItems(sourceDirList, targetPath);

                sourceDirList.Clear();
                var sourceFileEntriesList = m_SourceBackupPathList.Where(i => !i.IsFolder).ToList();
                foreach (var item in sourceFileEntriesList)
                {
                    sourceDirList.Add(m_IStorage.GetFileName(item.Path));
                }
                HandleDeletedFiles(sourceDirList, targetPath);
            }
            else
            {
                foreach (var item in m_Profile.BackupWatcherItemList)
                {
                    switch (item.ChangeType)
                    {
                        case WatcherChangeTypes.Changed:
                            {
                                ProcessFullBackupFile(item.Name, m_IStorage.GetDirectoryName(item.FullPath), targetPath);

                                break;
                            }

                        case WatcherChangeTypes.Created:
                            {

                                break;
                            }

                        case WatcherChangeTypes.Deleted:
                            {
                                //m_IStorage.DeleteFile(filePath);

                                break;
                            }

                        case WatcherChangeTypes.Renamed:
                            {
                                foreach (var backupPath in m_Profile.BackupFolderList)
                                {
                                    var cNewPath = item.FullPath;
                                    var bFoundNewPath = false;
                                    while (cNewPath != null)
                                    {
                                        if (cNewPath == backupPath.Path)
                                        {
                                            bFoundNewPath = true;
                                            break;
                                        }
                                        else
                                        {
                                            cNewPath = m_IStorage.GetDirectoryName(cNewPath);
                                        }
                                    }


                                    var cOldPath = item.OldPath;
                                    var bFoundOldPath = false;
                                    while (cOldPath != null)
                                    {
                                        if (cOldPath == backupPath.Path)
                                        {
                                            bFoundOldPath = true;
                                            break;
                                        }
                                        else
                                        {
                                            cOldPath = m_IStorage.GetDirectoryName(cOldPath);
                                        }
                                    }

                                    if (bFoundNewPath && bFoundOldPath)
                                    {

                                        var tNewPath = item.FullPath.Substring(m_IStorage.GetDirectoryName(cNewPath).Length + 1);
                                        var newTargetPath = m_IStorage.Combine(targetPath, tNewPath);

                                        var tOldPath = item.OldPath.Substring(m_IStorage.GetDirectoryName(cOldPath).Length + 1);
                                        var oldTargetPath = m_IStorage.Combine(targetPath, tOldPath);

                                        if (m_IStorage.FileExists(oldTargetPath))
                                        {
                                            if (!m_IStorage.FileExists(newTargetPath))
                                            {
                                                m_IStorage.MoveFile(oldTargetPath, newTargetPath);
                                            }
                                            else
                                            {
                                                LogConsoleView.Writeln($"Rename file, file not found in source folder {newTargetPath}");
                                            }
                                        }
                                        else
                                        {
                                            LogConsoleView.Writeln($"Rename file, file not found in backup folder {oldTargetPath}");
                                        }
                                    }
                                }

                                break;
                            }

                        default:
                            break;
                    }
                }
            }
        }

        protected override void ProcessFullBackupFile(string fileName, string sourcePath, string destPath)
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
                    m_IStorage.CopyFile(sourceFilePath, currSetFilePath, true);

                    m_BackupSessionHistory.AddUpdatedFile(sourceFilePath, currSetFilePath);
                }
            }
            else
            {
                if (!m_IStorage.DirectoryExists(destPath))
                {
                    m_IStorage.CreateDirectory(destPath);
                }
                m_IStorage.CopyFile(sourceFilePath, currSetFilePath);

                m_BackupSessionHistory.AddNewFile(sourceFilePath, currSetFilePath);
            }
        }

        protected override void ProcessFullBackupFolderStep(string sourcePath, string currSetPath)
        {
            var sourceFileList = m_IStorage.GetFiles(sourcePath);

            if (!m_IStorage.DirectoryExists(currSetPath))
            {
                m_IStorage.CreateDirectory(currSetPath);
            }

            foreach (var file in sourceFileList)
            {
                ProcessFullBackupFile(m_IStorage.GetFileName(file), sourcePath, currSetPath);
            }

            HandleDeletedFiles(sourceFileList, currSetPath);

            //Process directories
            var sourceSubdirectoryEntriesList = GetDirectoriesNames(sourcePath);

            foreach (string subdirectory in sourceSubdirectoryEntriesList)
            {
                string newSourceSetPath = m_IStorage.Combine(sourcePath, subdirectory);
                string newCurrSetPath = m_IStorage.Combine(currSetPath, subdirectory);

                ProcessFullBackupFolderStep(newSourceSetPath, newCurrSetPath);
            }

            HandleDeletedItems(sourceSubdirectoryEntriesList, currSetPath);
        }

    }
}
