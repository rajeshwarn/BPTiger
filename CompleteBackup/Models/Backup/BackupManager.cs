using CompleteBackup.Models.Backup.History;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace CompleteBackup.Models.backup
{
    public abstract class BackupManager
    {
        protected IStorageInterface m_IStorage;

        BackupManager() { }
        public BackupManager(List<string> sourcePath, string currSetPath, IStorageInterface storageInterface, GenericStatusBarView progressBar)
        {
            SourcePath = sourcePath;
            TargetPath = currSetPath;

            m_IStorage = storageInterface;
            m_ProgressBar = progressBar;
        }

        public abstract string BackUpProfileSignature { get; }

        public long NumberOfFiles { get; set; } = 0;
        public long ProcessFileCount { get; set; }

        public List<string> SourcePath;
        public string TargetPath;
        private GenericStatusBarView m_ProgressBar;
        public GenericStatusBarView ProgressBar { get { return m_ProgressBar; } set { } }

        DateTime m_LastProgressUpdate = DateTime.Now;
        public void UpdateProgress(string text, long progress)
        {
            if (ProgressBar != null)
            {
                DateTime dateTime = DateTime.Now;
                long milli = (dateTime.Ticks - m_LastProgressUpdate.Ticks) / TimeSpan.TicksPerMillisecond;
                if (milli >= 1000)
                {
                    ProgressBar?.UpdateProgressBar($"{text} {NumberOfFiles - ProcessFileCount} items left", progress);
                    m_LastProgressUpdate = dateTime;
                }
            }
        }

        protected BackupSessionHistory m_BackupSessionHistory = new BackupSessionHistory();


        public abstract void ProcessBackup();



        public void Init()
        {
            long files = 0;
            long directories = 0;

            foreach (var path in SourcePath)
            {
                long files_ = 0;
                long directories_ = 0;
                m_IStorage.GetNumberOfFiles(path, ref files_, ref directories_);

                directories += directories_;
                files += files_;
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




        protected void HandleFile(string sourcePath, string currSetPath, string lastSetPath, string fileName)
        {
            var sourceFilePath = m_IStorage.Combine(sourcePath, fileName);
            var lastSetFilePath = (lastSetPath == null) ? null : m_IStorage.Combine(lastSetPath, fileName);
            var currSetFilePath = m_IStorage.Combine(currSetPath, fileName);

            if (m_IStorage.FileExists(currSetFilePath))
            {
                if (m_IStorage.IsFileSame(sourceFilePath, currSetFilePath))
                {
                    //File is the same, do nothing
                    m_BackupSessionHistory.AddNoChangeFile(currSetFilePath);

                    m_BackupSessionHistory.AddNoChangeFile(sourcePath);
                }
                else
                {
                    //Move current file to old set
                    //if (!m_IStorage.DirectoryExists(lastSetPath))
                    //{
                    //    m_IStorage.CreateDirectory(lastSetPath);
                    //}

                    //Keep current version in set
                    m_IStorage.MoveFile(currSetFilePath, lastSetFilePath, true);

                    //Update new version to new set
                    if (!m_IStorage.DirectoryExists(currSetPath))
                    {
                        m_IStorage.CreateDirectory(currSetPath);
                    }
                    m_IStorage.CopyFile(sourceFilePath, currSetFilePath);

                    m_BackupSessionHistory.AddUpdatedFile(currSetFilePath);
                }
            }
            else
            {
                // new file, copy to current set
                if (!m_IStorage.DirectoryExists(currSetPath))
                {
                    m_IStorage.CreateDirectory(currSetPath);
                }
                m_IStorage.CopyFile(sourceFilePath, currSetFilePath);

                m_BackupSessionHistory.AddNewFile(currSetFilePath);
            }
        }

        protected void HandleFile(string sourcePath, string currSetPath, string fileName)
        {
            var sourceFilePath = m_IStorage.Combine(sourcePath, fileName);
            var currSetFilePath = m_IStorage.Combine(currSetPath, fileName);

            if (m_IStorage.FileExists(currSetFilePath))
            {
                if (m_IStorage.IsFileSame(sourceFilePath, currSetFilePath))
                {
                    //Do nothing
                    m_BackupSessionHistory.AddNoChangeFile(currSetFilePath);
                }
                else
                {
                    //update/overwrite file
                    m_IStorage.CopyFile(sourceFilePath, currSetFilePath, true);

                    m_BackupSessionHistory.AddUpdatedFile(currSetFilePath);
                }
            }
            else
            {
                if (!m_IStorage.DirectoryExists(currSetPath))
                {
                    m_IStorage.CreateDirectory(currSetPath);
                }
                m_IStorage.CopyFile(sourceFilePath, currSetFilePath);

                m_BackupSessionHistory.AddNewFile(currSetFilePath);
            }
        }

        protected void HandleDeletedFiles(List<string> sourceFileList, string currSetPath, string lastSetPath)
        {
            //Delete any deleted files
            var currSetFileList = m_IStorage.GetFiles(currSetPath);
            foreach (var filePath in currSetFileList)
            {
                var fileName = m_IStorage.GetFileName(filePath);
                if (!sourceFileList.Exists(item => m_IStorage.GetFileName(item) == fileName))
                {
                    //if not exists in source, delete the file
                    var prevSetfilePath = m_IStorage.Combine(lastSetPath, fileName);

                    //Move file to last set
                    m_IStorage.MoveFile(filePath, prevSetfilePath, true);

                    m_BackupSessionHistory.AddDeletedFile(filePath);
                }
            }
        }


        protected void HandleDeletedFiles(List<string> sourceFileList, string currSetPath)
        {
            //Delete any deleted files
            var currSetFileList = m_IStorage.GetFiles(currSetPath);
            foreach (var filePath in currSetFileList)
            {
                var fileName = m_IStorage.GetFileName(filePath);
                if (!sourceFileList.Exists(item => m_IStorage.GetFileName(item) == fileName))
                {
                    //if not exists in source, delete the file
                    File.Delete(filePath);

                    m_BackupSessionHistory.AddDeletedFile(filePath);
                }
            }
        }


        protected void HandleDeletedItems(List<string> sourceSubdirectoryEntriesList, string currSetPath, string lastSetPath)
        {
            //lookup for deleted items
            if (m_IStorage.DirectoryExists(currSetPath))
            {
                string[] targetSubdirectoryEntries = m_IStorage.GetDirectories(currSetPath);
                var deleteList = new List<string>();
                if (targetSubdirectoryEntries != null)
                {
                    foreach (var entry in targetSubdirectoryEntries)
                    {
                        if (!sourceSubdirectoryEntriesList.Exists(e => e == m_IStorage.GetFileName(entry)))
                        {
                            deleteList.Add(entry);
                        }
                    }
                }

                //Delete deleted items
                foreach (var entry in deleteList)
                {
                    m_IStorage.MoveDirectory(entry, m_IStorage.Combine(lastSetPath, m_IStorage.GetFileName(entry)), true);

                    m_BackupSessionHistory.AddDeletedFolder(entry);
                }
            }
        }

        protected void HandleDeletedItems(List<string> sourceSubdirectoryEntriesList, string currSetPath)
        {
            //lookup for deleted items
            if (m_IStorage.DirectoryExists(currSetPath))
            {
                string[] targetSubdirectoryEntries = m_IStorage.GetDirectories(currSetPath);
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
                    m_IStorage.DeleteDirectory(entry);

                    m_BackupSessionHistory.AddDeletedFolder(entry);
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
                FileAttributes attr = File.GetAttributes(newPath);
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
