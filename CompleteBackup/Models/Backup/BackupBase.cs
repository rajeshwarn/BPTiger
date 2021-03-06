﻿using CompleteBackup.Models.Backup;
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
    public abstract class BackupBase : BackupBaseFileSystem
    {
        protected IStorageInterface m_IStorage;
        protected BackupProfileData m_Profile;
        protected BackupPerfectLogger m_Logger;

        public BackupBase(BackupProfileData profile, GenericStatusBarView progressBar) : base(profile)
        {
            m_TimeStamp = DateTime.Now;
            m_Profile = profile;
            m_IStorage = profile.GetStorageInterface();
            m_Logger = profile.Logger;

            m_SourceBackupPathList = profile.BackupFolderList.ToList();//.Where(i => i.IsAvailable).ToList();
            m_TargetBackupPath = profile.GetTargetBackupFolder();

            m_ProgressBar = progressBar;
        }

        public abstract void ProcessBackup();

        public ManualResetEvent PauseWaitHandle { get; set; } = new ManualResetEvent(true);

        BackupProcessWorkerTask m_Task;


        public bool CheckCancellationPendingOrSleep()
        {
            bool bCancle = false;

            if (m_Task == null)
            {
                m_Task = BackupTaskManager.Instance.GetRunningBackupWorkerTask(m_Profile);
            }

            if (m_Task != null)
            {
                if (m_Task.CancellationPending)
                {
                    bCancle = true;
                }
                else
                {
                    m_Task.SleepIfNeeded();
                }
            }

            return bCancle;
        }

        private DateTime m_TimeStamp;
        protected DateTime GetTimeStamp()
        {
            return m_TimeStamp;
        }

        protected string GetTargetArchivePath(string path)
        {
            return m_IStorage.Combine(path, BackupProfileData.TargetBackupBaseDirectoryName);
        }
        protected string GetTargetSetNamePath()
        {
            return m_IStorage.Combine(GetTargetSetName(), BackupProfileData.TargetBackupBaseDirectoryName);
        }

        protected string GetTargetSetName()
        {
            return $"{m_Profile.BackupSignature}_{GetTimeStampString()}";
        }

        protected string GetTimeStampString()
        {
            return $"{m_TimeStamp.Year:0000}-{m_TimeStamp.Month:00}-{m_TimeStamp.Day:00}_{m_TimeStamp.Hour:00}{m_TimeStamp.Minute:00}{m_TimeStamp.Hour:00}{m_TimeStamp.Second:00}{m_TimeStamp.Millisecond:000}";
        }


        public long NumberOfFiles { get; set; } = 0;
        public long ProcessFileCount { get; set; }

        public List<FolderData> m_SourceBackupPathList;

        protected string m_TargetBackupPath;

        //protected string GetTargetBackupPath()
        //{
        //    return m_TargetBackupPath;
        //}
        protected string GetTargetBackupPathWithSetName(string path)
        {
            return m_IStorage.Combine(path, GetTargetSetName());
        }
        protected string GetTargetBackupPathWithSetPath(string path)
        {
            return m_IStorage.Combine(path, GetTargetSetNamePath());
        }



        protected string GetTargetBackupPathWithSetName()
        {
            return m_IStorage.Combine(GetTargetBackupFullPath(), GetTargetSetName());
        }
        protected string GetTargetBackupFullPath()
        {
            return m_IStorage.Combine(m_TargetBackupPath, GetTargetSetNamePath());
        }

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

        public static List<string> GetBackupSetList_(BackupProfileData profile)
        {
            var backupProfileList = new List<string>();
            try
            {
                var setEntries = profile?.GetStorageInterface().GetDirectories(profile.GetTargetBackupFolder())?.OrderBy(set => set);
                if (setEntries != null)
                {
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
                }
            }
            catch(DirectoryNotFoundException) { }

            return backupProfileList;
        }

        public static List<string> GetBackupSetWithTimeList_(BackupProfileData profile)
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

        public static List<string> GetBackupSetPathWithTimeList_(BackupProfileData profile)
        {
            var backupProfileList = new List<string>();
            var storage = profile.GetStorageInterface();

            string[] setEntries = storage.GetDirectories(profile.GetTargetBackupFolder());
            foreach (var entry in setEntries.Where(s => storage.GetFileName(s).StartsWith(profile.BackupSignature)))
            {
                backupProfileList.Add(profile.GetStorageInterface().Combine(storage.GetFileName(entry), BackupProfileData.TargetBackupBaseDirectoryName));
            }

            return backupProfileList.OrderByDescending(set => set).ToList();
        }

        public static string GetLastBackupSetPath_(BackupProfileData profile)
        {
            var name = GetLastBackupSetName_(profile);
            if (name != null)
            {
                name = profile.GetStorageInterface().Combine(name, BackupProfileData.TargetBackupBaseDirectoryName);
            }

            return name;
        }

        public static string GetLastBackupSetName_(BackupProfileData profile)
            {
                string lastSet = null;

            try
            {
                var setList = GetBackupSetList_(profile);
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
    }
}
