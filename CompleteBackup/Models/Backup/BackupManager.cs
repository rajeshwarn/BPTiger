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
    public abstract class BackupManager
    {
        protected IStorageInterface m_IStorage;
        protected BackupProfileData m_Profile;
        protected BackupSessionHistory m_BackupSessionHistory;
        protected BackupPerfectLogger m_Logger;

        public BackupManager(BackupProfileData profile, GenericStatusBarView progressBar)
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
            var setList = GetBackupSetList(profile);

            var lastSet = setList.FirstOrDefault();

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

        //protected void HandleFile(string sourcePath, string currSetPath, string fileName)
        //{
        //    var sourceFilePath = m_IStorage.Combine(sourcePath, fileName);
        //    var currSetFilePath = m_IStorage.Combine(currSetPath, fileName);

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
        //            CopyFile(sourceFilePath, currSetFilePath, true);

        //            m_BackupSessionHistory.AddUpdatedFile(sourceFilePath, currSetFilePath);
        //        }
        //    }
        //    else
        //    {
        //        if (!m_IStorage.DirectoryExists(currSetPath))
        //        {
        //            CreateDirectory(currSetPath);
        //        }
        //        CopyFile(sourceFilePath, currSetFilePath);

        //        m_BackupSessionHistory.AddNewFile(sourceFilePath, currSetFilePath);
        //    }
        //}

        protected virtual void HandleDeletedFiles(List<string> sourceFileList, string currSetPath)
        {
            //Delete any deleted files
            var currSetFileList = m_IStorage.GetFiles(currSetPath);
            foreach (var filePath in currSetFileList)
            {
                var fileName = m_IStorage.GetFileName(filePath);
                if (!sourceFileList.Exists(item => m_IStorage.GetFileName(item) == fileName))
                {
                    //if not exists in source, delete the file
                    DeleteFile(filePath);

                    m_BackupSessionHistory.AddDeletedFile(filePath, currSetPath);
                }
            }
        }

        protected void HandleDeletedItems(List<string> sourceSubdirectoryEntriesList, string currSetPath)
        {
            //lookup for deleted items
            if (m_IStorage.DirectoryExists(currSetPath))
            {
                string[] targetSubdirectoryEntries = m_IStorage.GetDirectoriesNames(currSetPath);
                var deleteList = new List<string>();
                foreach (var entry in targetSubdirectoryEntries)
                {
                    if (!sourceSubdirectoryEntriesList.Exists(e => e == m_IStorage.GetFileName(entry)))
                    {
                        deleteList.Add(entry);
                    }
                }

                //Delete deleted items
                foreach (var entry in deleteList)
                {
                    if (!entry.EndsWith(BackupSessionHistory.HistoryDirectory))
                    {
                        DeleteDirectory(m_IStorage.Combine(currSetPath, entry));

                        m_BackupSessionHistory.AddDeletedFolder(null, entry);
                    }
                }
            }
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

            return m_IStorage.MoveDirectory(sourcePath, targetPath, bCreateFolder);
        }

        protected bool DeleteDirectory(string path, bool bRecursive = true)
        {
            if (m_Profile.IsDetaledLog)
            {
                m_Logger.Writeln($"Delete Directory {path}");
            }
            else
            {
                m_Logger.Writeln($"Delete Directory {path}");
            }

            return m_IStorage.DeleteDirectory(path, bRecursive);
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
            if (m_Profile.IsDetaledLog)
            {
                m_Logger.Writeln($"Delete File {path}");
            }
            else
            {
                m_Logger.Writeln($"Delete File {path}");
            }

            m_IStorage.DeleteFile(path);
        }

        #endregion

    }
}
