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

 //           InitStorageDataUpdaterTask();
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






        //BackgroundWorker m_StorageDataUpdaterTask = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

        //void InitStorageDataUpdaterTask()
        //{
        //    m_StorageDataUpdaterTask.DoWork += (sender, e) =>
        //    {
        //        //var collection = e.Argument as EventCollectionDataBase;
        //        try
        //        {
        //            long files = 0;
        //            long directories = 0;

        //            Application.Current.Dispatcher.Invoke(new Action(() =>
        //            {
        //                NumberOfFiles = -1;
        //                ProgressBar?.SetPauseState(true);

        //            }));

        //            foreach (var path in SourcePath)
        //            {
        //                long files_ = 0;
        //                long directories_ = 0;
        //                m_IStorage.GetNumberOfFiles(path, ref files_, ref directories_);

        //                directories += directories_;
        //                files += files_;
        //            }

        //            Application.Current.Dispatcher.Invoke(new Action(() =>
        //            {
        //                NumberOfFiles = files;

        //                ProgressBar?.SetPauseState(false);
        //                ProgressBar?.SetRange(NumberOfFiles);
        //                ProgressBar?.UpdateProgressBar("Runnin...", 0);
        //                ProgressBar?.ShowTimeEllapsed(true);
        //            }));
        //        }
        //        catch (TaskCanceledException ex)
        //        {
        //            Trace.WriteLine($"StorageDataUpdaterTask exception: {ex.Message}");
        //            e.Result = $"StorageDataUpdaterTaskexception: {ex.Message}";
        //            throw (ex);
        //        }
        //    };
        //}












        XmlDocument m_xmlDoc;
        XmlNode m_NewItemsNode;
        XmlNode m_UpdatedItemsNode;
        XmlNode m_DeletedItemsNode;
        XmlNode m_NoChangeItemsNode;

        long m_iNewFiles = 0;
        long m_iUpdatedFiles = 0;
        long m_iDeletedFiles = 0;
        long m_iDeletedFolders = 0;
        long m_iNoChangeFiles = 0;

        public void CreateChangeLogFile()
        {
            m_xmlDoc = new XmlDocument();
            XmlNode rootNode = m_xmlDoc.CreateElement("ChangeLog");
            m_xmlDoc.AppendChild(rootNode);

            m_NewItemsNode = m_xmlDoc.CreateElement("New");
            m_UpdatedItemsNode = m_xmlDoc.CreateElement("Updated");
            m_DeletedItemsNode = m_xmlDoc.CreateElement("Deleted");
            m_NoChangeItemsNode = m_xmlDoc.CreateElement("NoChange");

            rootNode.AppendChild(m_NewItemsNode);
            rootNode.AppendChild(m_UpdatedItemsNode);
            rootNode.AppendChild(m_DeletedItemsNode);
            rootNode.AppendChild(m_NoChangeItemsNode);
        }

        public void SaveChangeLogFile(string path, string signature)
        {
            XmlAttribute attribute = m_xmlDoc.CreateAttribute("count");
            attribute.Value = m_iNoChangeFiles.ToString();
            m_NoChangeItemsNode.Attributes.Append(attribute);


            //            var tmpPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProject);
            //            var name = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{signature}_ChangeLog.xml");

            string targetFile = m_IStorage.Combine(path, $"{signature}_ChangeLog.xml");


            m_xmlDoc.Save(file);
            var storage = new FileSystemStorage();
            storage.MoveFile(file, targetFile);


        }

        public void AddNewFile(string item)
        {
            m_iNewFiles++;

            XmlNode userNode = m_xmlDoc.CreateElement("file");
            XmlAttribute attribute = m_xmlDoc.CreateAttribute("count");
            attribute.Value = m_iNewFiles.ToString();
            userNode.Attributes.Append(attribute);
            userNode.InnerText = item;
            m_NewItemsNode.AppendChild(userNode);
        }
        public void AddUpdatedFile(string item)
        {
            m_iUpdatedFiles++;

            XmlNode userNode = m_xmlDoc.CreateElement("file");
            XmlAttribute attribute = m_xmlDoc.CreateAttribute("count");
            attribute.Value = m_iUpdatedFiles.ToString();
            userNode.Attributes.Append(attribute);
            userNode.InnerText = item;
            m_UpdatedItemsNode.AppendChild(userNode);
        }
        public void AddDeletedFile(string item)
        {
            m_iDeletedFiles++;

            XmlNode userNode = m_xmlDoc.CreateElement("file");
            XmlAttribute attribute = m_xmlDoc.CreateAttribute("count");
            attribute.Value = m_iDeletedFiles.ToString();
            userNode.Attributes.Append(attribute);
            userNode.InnerText = item;
            m_DeletedItemsNode.AppendChild(userNode);
        }

        public void AddDeletedFolder(string item)
        {
            m_iDeletedFolders++;

            XmlNode userNode = m_xmlDoc.CreateElement("folder");
            XmlAttribute attribute = m_xmlDoc.CreateAttribute("count");
            attribute.Value = m_iDeletedFolders.ToString();
            userNode.Attributes.Append(attribute);
            userNode.InnerText = item;
            m_DeletedItemsNode.AppendChild(userNode);
        }

        public void AddNoChangeFile(string item)
        {
            m_iNoChangeFiles++;

            //XmlNode userNode = m_xmlDoc.CreateElement("file");
            //XmlAttribute attribute = m_xmlDoc.CreateAttribute("count");
            //attribute.Value = m_iNoChangeFiles.ToString();
            //userNode.Attributes.Append(attribute);
            //userNode.InnerText = item;
            //m_NoChangeItemsNode.AppendChild(userNode);
        }




        public abstract void ProcessBackup();
        //public abstract void ProcessBackupStep(string sourcePath, string currSetPath, string lastSetPath = null);

        //BackgroundWorker m_Worker = null;

        //public void StartNewWorker(string sourcePath, string currSetPath, string lastSetPath = null)
        //{
        //    ProcessBackupStep(sourcePath, currSetPath, lastSetPath);

        //    return;

        //    if (m_Worker != null && m_Worker.IsBusy)
        //    {
        //        ProcessBackupStep(sourcePath, currSetPath, lastSetPath);
        //    }
        //    else
        //    {
        //        m_Worker = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

        //        m_Worker.DoWork += (sender, e) =>
        //        {
        //            try
        //            {
        //                ProcessBackupStep(sourcePath, currSetPath, lastSetPath);
        //            }
        //            catch (TaskCanceledException ex)
        //            {
        //                Trace.WriteLine($"Backup Worker exception: {ex.Message}");
        //                e.Result = $"Backup Worker exception: {ex.Message}";
        //                throw (ex);
        //            }
        //        };

        //        m_Worker.RunWorkerAsync();
        //    }
        //}



        public void Init()
        {
            //            if (!m_StorageDataUpdaterTask.IsBusy)
            //            {
            //                m_StorageDataUpdaterTask.RunWorkerAsync();
            //       
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
                    AddNoChangeFile(currSetFilePath);
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

                    AddUpdatedFile(currSetFilePath);
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

                AddNewFile(currSetFilePath);
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
                    AddNoChangeFile(currSetFilePath);
                }
                else
                {
                    //update/overwrite file
                    m_IStorage.CopyFile(sourceFilePath, currSetFilePath, true);

                    AddUpdatedFile(currSetFilePath);
                }
            }
            else
            {
                if (!m_IStorage.DirectoryExists(currSetPath))
                {
                    m_IStorage.CreateDirectory(currSetPath);
                }
                m_IStorage.CopyFile(sourceFilePath, currSetFilePath);

                AddNewFile(currSetFilePath);
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

                    AddDeletedFile(filePath);
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

                    AddDeletedFile(filePath);
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

                    AddDeletedFolder(entry);
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

                    AddDeletedFolder(entry);
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
