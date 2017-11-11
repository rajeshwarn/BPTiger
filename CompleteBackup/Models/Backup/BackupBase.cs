using CompleteBackup.Models.Backup;
using CompleteBackup.Models.Backup.History;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Models.Utilities;
using CompleteBackup.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace CompleteBackup.Models.backup
{
    public abstract class BackupBase
    {
        protected IStorageInterface m_IStorage;
        protected BackupProfileData m_Profile;
        protected BackupSessionHistory m_BackupSessionHistory;
        protected BackupPerfectLogger m_Logger;

        public BackupBase(BackupProfileData profile, GenericStatusBarView progressBar)
        {
            m_TimeStamp = DateTime.Now;
            m_Profile = profile;
            m_SourceBackupPathList = profile.BackupFolderList.ToList();//.Where(i => i.IsAvailable).ToList();
            m_TargetBackupPath = profile.TargetBackupFolder;

            m_IStorage = profile.GetStorageInterface();
            m_Logger = profile.Logger;
            m_BackupSessionHistory = new BackupSessionHistory(profile.GetStorageInterface());

            m_ProgressBar = progressBar;
        }


        public ManualResetEvent PauseWaitHandle { get; set; } = new ManualResetEvent(true);

        //        public abstract string BackUpProfileSignature { get; }

        private DateTime m_TimeStamp;
        protected DateTime GetTimeStamp()
        {
            return m_TimeStamp;
        }


        protected string GetTargetSetName()
        {
            return $"{m_Profile.BackupSignature}_{GetTimeStampString()}";
        }

        protected string GetTimeStampString()
        {
            return  $"{m_TimeStamp.Year:0000}-{m_TimeStamp.Month:00}-{m_TimeStamp.Day:00}_{m_TimeStamp.Hour:00}{m_TimeStamp.Minute:00}{m_TimeStamp.Hour:00}{m_TimeStamp.Second:00}{m_TimeStamp.Millisecond:000}";
        }


        public long NumberOfFiles { get; set; } = 0;
        public long ProcessFileCount { get; set; }

        public List<FolderData> m_SourceBackupPathList;

        public string m_TargetBackupPath;

        private GenericStatusBarView m_ProgressBar;
        public GenericStatusBarView ProgressBar { get { return m_ProgressBar; } set { } }

        DateTime m_LastProgressUpdate = DateTime.Now;
        public void UpdateProgress(string text, long progress, string fileName)
        {
            if (!PauseWaitHandle.WaitOne(0))
            {
                DateTime dateTime = DateTime.Now;
                PauseWaitHandle.WaitOne();
            }

            if (ProgressBar != null)
            {
                DateTime dateTime = DateTime.Now;
                long milli = (dateTime.Ticks - m_LastProgressUpdate.Ticks) / TimeSpan.TicksPerMillisecond;
                if (milli >= 500)
                {
                    m_Profile.CurrentBackupFile = fileName;

                    if (milli >= 1000)
                    {
                        ProgressBar?.UpdateProgressBar($"{text} {NumberOfFiles - ProcessFileCount} items left", progress);
                        m_LastProgressUpdate = dateTime;
                    }
                }
            }
        }


        public abstract void ProcessBackup();


        public static List<string> GetBackupSetList(BackupProfileData profile)
        {
            var backupProfileList = new List<string>();
            var storage = profile.GetStorageInterface();

            string[] setEntries = storage.GetDirectories(profile.TargetBackupFolder);            
            foreach (var entry in setEntries.Where(s => storage.GetFileName(s).StartsWith(profile.BackupSignature)))
            {
                backupProfileList.Add(storage.GetFileName(entry));
            }

            return backupProfileList.OrderByDescending(set => set).ToList();
        }
        public static List<string> GetBackupSetWithTimeList(BackupProfileData profile)
        {
            var backupProfileList = new List<string>();
            var storage = profile.GetStorageInterface();

            string[] setEntries = storage.GetDirectories(profile.TargetBackupFolder);
            foreach (var entry in setEntries.Where(s => storage.GetFileName(s).StartsWith(profile.BackupSignature)))
            {
                backupProfileList.Add(storage.GetFileName(entry));
            }

            return backupProfileList.OrderByDescending(set => set).ToList();
        }

        public static string GetLastBackupSetName(BackupProfileData profile)
        {
            string lastSet = null;

            try
            {
                var setList = GetBackupSetList(profile);
                lastSet = setList.FirstOrDefault();
            }
            catch (DirectoryNotFoundException)
            { }

            return lastSet;
        }

        public void Init()
        {
            long files = 0;
            long directories = 0;

            foreach (var item in m_SourceBackupPathList)
            {
                if (item.IsFolder)
                {
                    try
                    {
                        files += m_IStorage.GetNumberOfFiles(item.Path);
                        directories += m_IStorage.GetNumberOfDirectories(item.Path);
                    }
                    catch (DirectoryNotFoundException) { }
                }
                else
                {
                    files++;
                }
            }

            NumberOfFiles = files;

            ProgressBar?.SetRange(NumberOfFiles);
            ProgressBar?.UpdateProgressBar("Runnin...", 0);
            ProgressBar?.ShowTimeEllapsed(true);
        }
        

        public void Done()
        {
            ProgressBar.ShowTimeEllapsed(false);
            ProgressBar?.UpdateProgressBar("Done...", NumberOfFiles);
            ProgressBar?.Release();
        }

        protected List<string> GetDirectoriesNames(string path)
        {
            //Process directories
            string[] sourceSubdirectoryEntries = m_IStorage.GetDirectories(path);

            var sourceSubdirectoryEntriesList = new List<string>();
            foreach (var entry in sourceSubdirectoryEntries)
            {
                string newPath = m_IStorage.Combine(path, entry);
                FileAttributes attr = m_IStorage.GetFileAttributes(newPath);
                if (((attr & FileAttributes.System) != FileAttributes.System) &&
                    ((attr & FileAttributes.Hidden) != FileAttributes.Hidden))
                {
                    sourceSubdirectoryEntriesList.Add(m_IStorage.GetFileName(entry));
                }
            }

            return sourceSubdirectoryEntriesList;
        }


        private bool? CheckItemTypeFolderOrFile(string path, WatcherChangeTypes changeType)
        {
            bool? bFolder = null;
            try
            {
                bFolder = m_IStorage.IsFolder(path);
            }
            catch (FileNotFoundException)
            {
                m_Logger.Writeln($"**Error {changeType} File, File not found {path}");
            }
            catch (DirectoryNotFoundException)
            {
                m_Logger.Writeln($"**Error {changeType} Directory, Directory not found {path}");
            }

            return bFolder;
        }


        List<FileSystemWatcherItemData> GetFilterBackupWatcherItemList(List<FileSystemWatcherItemData> itemList) 
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

        protected void ProcessBackupWatcherRootFolders(string targetPath, string lastTargetPath = null)
        {
            List<FileSystemWatcherItemData> itemList = null;

            lock (m_Profile)
            {
                itemList = m_Profile.BackupWatcherItemList.ToList();
                m_Profile.BackupWatcherItemList.Clear();
            }

            itemList = GetFilterBackupWatcherItemList(itemList);

            foreach (var item in itemList)
            {
                var iRetry = 50;
                do
                {
                    try
                    {
                        int index = itemList.IndexOf(item);

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

                                            CopyFile(item.FullPath, newTargetPath);
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
                                                //delete old version if no old set exists, happens with full backup and incremental
                                                DeleteFile(newTargetPath);
                                            }
                                            CopyFile(item.FullPath, newTargetPath, true);
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
                                                    CopyFile(oldTargetPath, newLastTargetPath, true);
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

                        iRetry = 0;
                    }
                    catch (IOException)
                    {
                        iRetry -= 1;
                        Thread.Sleep(100);
                    }
                }
                while (iRetry > 0);
            }

//            m_Profile.BackupWatcherItemList.Clear();
        }

        #region File System wrappers
        protected void CopyFile(string sourcePath, string targetPath, bool overwrite = false)
        {
            if (m_Profile.IsDetaledLog)
            {
                m_Logger.Writeln($"File Copy {sourcePath} To {targetPath}");
            }
            else
            {
                m_Logger.Writeln($"File Copy {sourcePath}");
            }

            m_IStorage.CopyFile(sourcePath, targetPath, overwrite);
        }

        protected void CreateDirectory(string path, bool bCheckIfExist = false)
        {
            if (m_Profile.IsDetaledLog)
            {
                m_Logger.Writeln($"Create Directory {path}");
            }
            else
            {
                m_Logger.Writeln($"Create Directory {path}");
            }

            m_IStorage.CreateDirectory(path, bCheckIfExist);
        }

        protected bool MoveDirectory(string sourcePath, string targetPath, bool bCreateFolder = false)
        {
            if (m_Profile.IsDetaledLog)
            {
                m_Logger.Writeln($"Move Directory {sourcePath} To {targetPath}");
            }
            else
            {
                m_Logger.Writeln($"Move Directory {sourcePath}");
            }

            var parentDir = m_IStorage.GetDirectoryName(targetPath);
            if (!m_IStorage.DirectoryExists(parentDir))
            {
                CreateDirectory(parentDir);
            }

            return m_IStorage.MoveDirectory(sourcePath, targetPath, bCreateFolder);
        }

        protected bool DeleteDirectory(string path, bool bRecursive = true)
        {
            bool bRet = false;
            try
            {
                bRet = m_IStorage.DeleteDirectory(path, bRecursive);
                m_Logger.Writeln($"Delete Directory {path}");
            }
            catch (UnauthorizedAccessException)
            {
                m_Logger.Writeln($"Delete Read only Directory: {path}");
                m_IStorage.SetFileAttributeRecrusive(path, FileAttributes.Normal);
                bRet = m_IStorage.DeleteDirectory(path, bRecursive);
            }

            return bRet;
        }

        protected void MoveFile(string sourcePath, string targetPath, bool bCreateFolder = false)
        {
            if (m_Profile.IsDetaledLog)
            {
                m_Logger.Writeln($"Move File {sourcePath} To {targetPath}");
            }
            else
            {
                m_Logger.Writeln($"Move File {sourcePath}");
            }

            m_IStorage.MoveFile(sourcePath, targetPath, bCreateFolder);
        }
        protected void DeleteFile(string path)
        {
            try
            {
                m_IStorage.DeleteFile(path);
                m_Logger.Writeln($"Delete File {path}");
            }
            catch (UnauthorizedAccessException)
            {
                m_Logger.Writeln($"Delete Read only file File {path}");
                m_IStorage.SetFileAttribute(path, FileAttributes.Normal);
                m_IStorage.DeleteFile(path);
            }
        }

        #endregion

    }
}
