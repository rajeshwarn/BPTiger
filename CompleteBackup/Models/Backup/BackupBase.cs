using CompleteBackup.Models.Backup;
using CompleteBackup.Models.Backup.History;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Models.Profile;
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
            m_IStorage = profile.GetStorageInterface();
            m_Logger = profile.Logger;

            m_SourceBackupPathList = profile.BackupFolderList.ToList();//.Where(i => i.IsAvailable).ToList();
            m_TargetBackupPath = GetTargetSetArchivePath(profile);

            m_BackupSessionHistory = new BackupSessionHistory(profile.GetStorageInterface());

            m_ProgressBar = progressBar;
        }

        public abstract void ProcessBackup();

        public ManualResetEvent PauseWaitHandle { get; set; } = new ManualResetEvent(true);

        BackupProcessWorkerTask m_Task;

        public bool CheckCancellationPending()
        {
            bool bCancle = false;

            if (m_Task == null)
            {
                m_Task = BackupTaskManager.Instance.GetRunningBackupWorkerTask(m_Profile);
            }

            if ((m_Task != null) && m_Task.CancellationPending)
            {
                bCancle = true;
            }

            return bCancle;
        }

        private DateTime m_TimeStamp;
        protected DateTime GetTimeStamp()
        {
            return m_TimeStamp;
        }


        public static string TargetBackupBaseDirectoryName { get; } = "Archive";


        protected static string GetTargetSetArchivePath(BackupProfileData profile)
        {
            return profile.GetStorageInterface().Combine("Archive", profile.GetTargetBackupFolder());
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
                    //m_Profile.CurrentBackupFile = fileName;

                    if (milli >= 1000)
                    {
                        ProgressBar?.UpdateProgressBar($"{text} {NumberOfFiles - ProcessFileCount} items left", progress);
                        m_LastProgressUpdate = dateTime;
                    }
                }
            }

            if (m_Task != null)
            {
                m_Task.ReportProgress((int)progress);
            }
        }

        public static List<string> GetBackupSetList(BackupProfileData profile)
        {
            var backupProfileList = new List<string>();
            var setEntries = profile.GetStorageInterface().GetDirectories(profile.GetTargetBackupFolder()).OrderBy(set => set);

            if (profile.FileSystemWatcherEnabled)
            {
                foreach (var entry in setEntries)
                {
                    backupProfileList.Add(profile.GetStorageInterface().GetFileName(entry));
                }
            }
            else
            {
                var entry = setEntries.LastOrDefault();
                if (entry != null)
                {
                    backupProfileList.Add(profile.GetStorageInterface().GetFileName(entry));
                }
            }

            return backupProfileList;
        }

        public static List<string> GetBackupSetWithTimeList(BackupProfileData profile)
        {
            var backupProfileList = new List<string>();
            var storage = profile.GetStorageInterface();

            string[] setEntries = storage.GetDirectories(profile.GetTargetBackupFolder());
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
                lastSet = setList.LastOrDefault();
            }
            catch (DirectoryNotFoundException)
            { }

            return lastSet;
        }

        public virtual void Init()
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
        

        public virtual void Done()
        {
            ProgressBar.ShowTimeEllapsed(false);
            ProgressBar?.UpdateProgressBar("Done...", NumberOfFiles);
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

            try
            {
                m_IStorage.CreateDirectory(path, bCheckIfExist);
            }
            catch (Exception ex)
            {
                m_Logger.Writeln($"**Exception while creating directory: {path}\n{ex.Message}");
            }
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
            catch (Exception ex)
            {
                m_Logger.Writeln($"**Exception while deleteing directory: {path}\n{ex.Message}");
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
            catch (Exception ex)
            {
                m_Logger.Writeln($"**Exception while deleteing file: {path}\n{ex.Message}");
            }
        }

        #endregion

    }
}
