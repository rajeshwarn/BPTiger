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
    public class IncrementalFullWatcherBackup : IncrementalFullBackup
    {
        public IncrementalFullWatcherBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar)
        {
            m_IStorage = new FileSystemStorage();
        }

        //public override void ProcessBackup()
        //{
        //    m_BackupSessionHistory.Reset(GetTimeStamp());
        //    //path = profile.GetStorageInterface().Combine(profile.TargetBackupFolder, path);
        //    var backupName = BackupManager.GetLastBackupSetName(m_Profile);
        //    if (backupName == null)
        //    {
        //        //this is the first run
        //        backupName = GetTargetSetName();
        //        CreateNewBackupSetFolder(backupName);
        //    }

        //    var targetSetPath = m_IStorage.Combine(m_TargetBackupPath, backupName);

        //    ProcessBackupRootFolders(targetSetPath);

        //    BackupSessionHistory.SaveHistory(m_TargetBackupPath, backupName, m_BackupSessionHistory);//, GetTimeStampString());
        //}


        protected override void ProcessBackupRootFolders(string targetPath)
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
                            var sourcePath = item.FullPath;
                            var newTargetPath = m_IStorage.Combine(m_IStorage.Combine(targetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);
                            if (m_IStorage.IsFolder(item.FullPath))
                            {
                                if (!m_IStorage.DirectoryExists(newTargetPath))
                                {
                                    CreateDirectory(newTargetPath);
                                }
                                else
                                {
                                    m_Logger.Writeln($"**Create Directory, Can't create - directory already exists {newTargetPath}");
                                }
                            }
                            else
                            {
                                if (!m_IStorage.FileExists(newTargetPath))
                                {
                                    CopyFile(sourcePath, newTargetPath);
                                }
                                else
                                {
                                    m_Logger.Writeln($"**Create File, Can't create - file already exists {newTargetPath}");
                                }
                            }

                            break;
                        }

                    case WatcherChangeTypes.Deleted:
                        {
                            //m_IStorage.DeleteFile(filePath);

                            break;
                        }

                    case WatcherChangeTypes.Renamed:
                        {
                            if (m_IStorage.IsFolder(item.WatchPath))
                            {
                                var newTargetPath = m_IStorage.Combine(m_IStorage.Combine(targetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);
                                var oldTargetPath = m_IStorage.Combine(m_IStorage.GetDirectoryName(newTargetPath), m_IStorage.GetFileName(item.OldPath));

                                if (m_IStorage.IsFolder(oldTargetPath))
                                {
                                    if (m_IStorage.DirectoryExists(oldTargetPath))
                                    {
                                        if (!m_IStorage.DirectoryExists(newTargetPath))
                                        {
                                            MoveDirectory(oldTargetPath, newTargetPath);
                                        }
                                        else
                                        {
                                            m_Logger.Writeln($"**Rename Directory, Can't rename - directory already exists {newTargetPath}");
                                        }
                                    }
                                    else
                                    {
                                        m_Logger.Writeln($"**Rename Directory, Can't rename - Directory not found in backup folder {oldTargetPath}");
                                    }
                                }
                                else if (m_IStorage.FileExists(oldTargetPath))
                                {
                                    if (!m_IStorage.FileExists(newTargetPath))
                                    {
                                        MoveFile(oldTargetPath, newTargetPath);
                                    }
                                    else
                                    {
                                        m_Logger.Writeln($"**Rename File, Can't rename - file already exists {newTargetPath}");
                                    }
                                }
                                else
                                {
                                    m_Logger.Writeln($"**Rename file, Can't rename - file not found in backup folder {oldTargetPath}");
                                }
                            }

                            break;
                        }

                    default:
                        break;
                }
            }            
        }

        //protected override void ProcessFullBackupFile(string fileName, string sourcePath, string destPath)
        //{
        //    var sourceFilePath = m_IStorage.Combine(sourcePath, fileName);
        //    var currSetFilePath = m_IStorage.Combine(destPath, fileName);

        //    UpdateProgress("Running... ", ++ProcessFileCount, sourceFilePath);

        //    if (m_IStorage.FileExists(currSetFilePath))
        //    {
        //        if (m_IStorage.IsFileSame(sourceFilePath, currSetFilePath))
        //        {
        //            //Do nothing
        //            m_BackupSessionHistory.AddNoChangeFile(sourceFilePath, currSetFilePath);
        //        }
        //        else
        //        {
        //            //update/overwrite file
        //            m_IStorage.CopyFile(sourceFilePath, currSetFilePath, true);

        //            m_BackupSessionHistory.AddUpdatedFile(sourceFilePath, currSetFilePath);
        //        }
        //    }
        //    else
        //    {
        //        if (!m_IStorage.DirectoryExists(destPath))
        //        {
        //            m_IStorage.CreateDirectory(destPath);
        //        }
        //        m_IStorage.CopyFile(sourceFilePath, currSetFilePath);

        //        m_BackupSessionHistory.AddNewFile(sourceFilePath, currSetFilePath);
        //    }
        //}

        //protected override void ProcessFullBackupFolderStep(string sourcePath, string currSetPath)
        //{
        //    var sourceFileList = m_IStorage.GetFiles(sourcePath);

        //    if (!m_IStorage.DirectoryExists(currSetPath))
        //    {
        //        m_IStorage.CreateDirectory(currSetPath);
        //    }

        //    foreach (var file in sourceFileList)
        //    {
        //        ProcessFullBackupFile(m_IStorage.GetFileName(file), sourcePath, currSetPath);
        //    }

        //    HandleDeletedFiles(sourceFileList, currSetPath);

        //    //Process directories
        //    var sourceSubdirectoryEntriesList = GetDirectoriesNames(sourcePath);

        //    foreach (string subdirectory in sourceSubdirectoryEntriesList)
        //    {
        //        string newSourceSetPath = m_IStorage.Combine(sourcePath, subdirectory);
        //        string newCurrSetPath = m_IStorage.Combine(currSetPath, subdirectory);

        //        ProcessFullBackupFolderStep(newSourceSetPath, newCurrSetPath);
        //    }

        //    HandleDeletedItems(sourceSubdirectoryEntriesList, currSetPath);
        //}

    }
}
