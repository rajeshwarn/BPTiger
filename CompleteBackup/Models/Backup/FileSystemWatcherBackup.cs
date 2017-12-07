using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models.Backup
{
    public class FileSystemWatcherBackup : IncrementalFullBackup
    {
        public FileSystemWatcherBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar) { }

        public override void Init()
        {
            lock (m_Profile)
            {
                m_WatcherItemList = m_Profile.BackupWatcherItemList.ToList();
                m_Profile.BackupWatcherItemList.Clear();
            }

            m_WatcherItemList = GetFilterBackupWatcherItemList(m_WatcherItemList);

            NumberOfFiles = m_WatcherItemList.LongCount();

            ProgressBar?.SetRange(NumberOfFiles);
            ProgressBar?.UpdateProgressBar("Runnin...", 0);
            ProgressBar?.ShowTimeEllapsed(true);
        }

        protected List<FileSystemWatcherItemData> GetFilterBackupWatcherItemList(List<FileSystemWatcherItemData> itemList)
        {
            var filterList = new List<Backup.FileSystemWatcherItemData>();

            FileSystemWatcherItemData lastItem = null;
            foreach (var item in itemList)
            {
                if ((lastItem?.ChangeType == WatcherChangeTypes.Created) &&
                    (item.ChangeType == WatcherChangeTypes.Changed) &&
                    (lastItem?.FullPath == item.FullPath))
                {
                    //do nothing
                }
                else if ((lastItem?.ChangeType == WatcherChangeTypes.Changed) &&
                    (item.ChangeType == WatcherChangeTypes.Changed) &&
                    (lastItem?.FullPath == item.FullPath))
                {
                    //do nothing
                }
                else
                {
                    filterList.Add(item);
                    lastItem = item;
                }
            }

            return filterList;
        }

        protected List<FileSystemWatcherItemData> m_WatcherItemList = null;
        //bool? IsFileinUse(FileSystemWatcherItemData item)
        //{
        //    bool? bUse = false;
        //    FileStream stream = null;

        //    if (item.ChangeType == WatcherChangeTypes.Deleted)
        //    {
        //        bUse = null;
        //    }
        //    else
        //    {
        //        try
        //        {
        //            var fileInfo = new System.IO.FileInfo(item.FullPath);
        //            stream = fileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        //            stream.Close();
        //        }
        //        catch (UnauthorizedAccessException)
        //        {
        //            bUse = true;
        //        }
        //        catch (IOException)
        //        {
        //            bUse = true;
        //        }
        //    }

        //    return bUse;
        //}

        private bool? CheckItemTypeFolderOrFile(string path, WatcherChangeTypes changeType)
        {
            bool? bFolder = null;
            try
            {
                bFolder = m_IStorage.IsFolder(path);
            }
            catch (FileNotFoundException)
            {
                m_Logger.Writeln($"***Exception {changeType} File, File not found {path}");
            }
            catch (DirectoryNotFoundException)
            {
                m_Logger.Writeln($"***Exception {changeType} Directory, Directory not found {path}");
            }

            return bFolder;
        }

        protected override void ProcessBackupRootFolders(string targetPath, string lastTargetPath = null)
        {
            long iItemCount = 0;
            foreach (var item in m_WatcherItemList)
            {
                if (CheckCancellationPendingOrSleep()) { return; }

                UpdateProgress("Running... ", ++iItemCount, item.Name);

                //if (IsFileinUse(item) != true)
                try
                {
                    ProcessFileSystemWatchItem(item, targetPath, lastTargetPath);
                }
                catch (Exception ex)
                {
                    m_Logger.Writeln($"**Exception while processing watch item {item.ChangeType}, path: {item.FullPath} [old path: {item.OldPath}]\n{ex.Message}");
                }
            }
        }

        protected void ProcessFileSystemWatchItem(FileSystemWatcherItemData item, string targetPath, string lastTargetPath = null)
        {
            switch (item.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    {
                        bool? bFolder = CheckItemTypeFolderOrFile(item.FullPath, item.ChangeType);
                        if (bFolder == false)
                        {
                            var newTargetPath = m_IStorage.Combine(m_IStorage.Combine(targetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);
                            if (!m_IStorage.FileExists(newTargetPath))
                            {
                                m_Logger.Writeln($"**Update File, Warning - file does not exist, will copy the updated version {newTargetPath}");

                                var targetDir = m_IStorage.GetDirectoryName(newTargetPath);
                                if (!m_IStorage.DirectoryExists(targetDir))
                                {
                                    CreateDirectory(targetDir);
                                }

                                CopyUpdatedFile(item.FullPath, newTargetPath);
                            }
                            else
                            {
                                if (lastTargetPath != null)
                                {
                                    var newLastTargetPath = m_IStorage.Combine(m_IStorage.Combine(lastTargetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);
                                    if (m_IStorage.FileExists(newLastTargetPath))
                                    {
                                        m_IStorage.DeleteFile(newLastTargetPath);
                                    }
                                    MoveFile(newTargetPath, newLastTargetPath, true);
                                }
                                else
                                {
                                    //delete old version if old set not exists, happens with full backup and incremental
                                    DeleteFile(newTargetPath);
                                }
                                CopyUpdatedFile(item.FullPath, newTargetPath, true);
                            }

                        }

                        break;
                    }

                case WatcherChangeTypes.Created:
                    {
                        var sourcePath = item.FullPath;
                        var newTargetPath = m_IStorage.Combine(m_IStorage.Combine(targetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);

                        bool? bFolder = CheckItemTypeFolderOrFile(item.FullPath, item.ChangeType);
                        if (bFolder == true)
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
                        else if (bFolder == false)
                        {
                            if (!m_IStorage.FileExists(newTargetPath))
                            {
                                var dirName = m_IStorage.GetDirectoryName(newTargetPath);
                                if (!m_IStorage.DirectoryExists(dirName))
                                {
                                    CreateDirectory(dirName);
                                }
                                CopyNewFile(sourcePath, newTargetPath);
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
                        var sourcePath = item.FullPath;
                        var newTargetPath = m_IStorage.Combine(m_IStorage.Combine(targetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);

                        bool? bFolder = CheckItemTypeFolderOrFile(newTargetPath, item.ChangeType);
                        if (bFolder == true)
                        {
                            if (m_IStorage.DirectoryExists(newTargetPath))
                            {
                                if (lastTargetPath == null)
                                {
                                    DeleteDirectory(newTargetPath);
                                }
                                else
                                {
                                    var newLastTargetPath = m_IStorage.Combine(m_IStorage.Combine(lastTargetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);
                                    MoveDirectory(newTargetPath, newLastTargetPath);
                                }
                            }
                            else
                            {
                                m_Logger.Writeln($"**Delete Directory, Can't delete - directory not found {newTargetPath}");
                            }
                        }
                        else if (bFolder == false)
                        {
                            if (m_IStorage.FileExists(newTargetPath))
                            {
                                if (lastTargetPath == null)
                                {
                                    DeleteFile(newTargetPath);
                                }
                                else
                                {
                                    var newLastTargetPath = m_IStorage.Combine(m_IStorage.Combine(lastTargetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);
                                    MoveFile(newTargetPath, newLastTargetPath, true);
                                }
                            }
                            else
                            {
                                m_Logger.Writeln($"**Delete File, Can't delete - file not found {newTargetPath}");
                            }
                        }
                        break;
                    }

                case WatcherChangeTypes.Renamed:
                    {
                        bool? bFolder = CheckItemTypeFolderOrFile(item.FullPath, item.ChangeType);
                        if (bFolder != null)
                        {
                            var newTargetPath = m_IStorage.Combine(m_IStorage.Combine(targetPath, m_IStorage.GetFileName(item.WatchPath)), item.Name);
                            var oldTargetPath = m_IStorage.Combine(m_IStorage.GetDirectoryName(newTargetPath), m_IStorage.GetFileName(item.OldPath));

                            if (bFolder == true)
                            {
                                if (m_IStorage.DirectoryExists(oldTargetPath))
                                {
                                    if (!m_IStorage.DirectoryExists(newTargetPath))
                                    {
                                        if (lastTargetPath == null)
                                        {
                                            MoveDirectory(oldTargetPath, newTargetPath);
                                        }
                                        else
                                        { //TODO!!!!!!!
                                            //CopyDirectory(oldTargetPath, newLastTargetPath);
                                            var newLastTargetPath = m_IStorage.Combine(m_IStorage.Combine(lastTargetPath, m_IStorage.GetFileName(item.WatchPath)), m_IStorage.GetFileName(item.Name));
                                            //CopyDirectory(oldTargetPath, newLastTargetPath);

                                            MoveDirectory(oldTargetPath, newTargetPath);
                                        }
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
                                    if (lastTargetPath != null)
                                    {
                                        var newLastTargetPath = m_IStorage.Combine(m_IStorage.Combine(lastTargetPath, m_IStorage.GetFileName(item.WatchPath)), m_IStorage.GetFileName(item.OldPath));
                                        CopyRenamedFile(oldTargetPath, newLastTargetPath, true);
                                    }
                                    else
                                    {
                                        MoveFile(oldTargetPath, newTargetPath);
                                    }
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
}
