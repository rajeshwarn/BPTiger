using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.History;
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
    public class OneWaySyncBackup : BackupManager
    {
        public OneWaySyncBackup(string backupName, List<string> sourcePath, string currSetPath, IStorageInterface storageInterface, GenericStatusBarView progressBar = null) : base(sourcePath, currSetPath, storageInterface, progressBar)
        {
            m_IStorage = new FileSystemStorage();
            m_BackupName = backupName;
        }
        public override string BackUpProfileSignature { get { return $"{BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile?.GUID.ToString("D")}-CBKP-SNAP"; } }
        string m_BackupName;

        public override void ProcessBackup()
        {
            if (m_BackupName == null)
            {
                DateTime d = DateTime.Now;
                string dateSignature = $"{d.Year:0000}-{d.Month:00}-{d.Day:00}_{d.Hour:00}{d.Minute:00}{d.Hour:00}{d.Second:00}{d.Millisecond:000}";
                m_BackupName = $"{BackUpProfileSignature}_{dateSignature}";
            }

            var newTargetPath = m_IStorage.Combine(TargetPath, m_BackupName);
    
            m_IStorage.CreateDirectory(newTargetPath);

            m_BackupSessionHistory.Clear();

            foreach (var path in SourcePath)
            {
                var targetdirectoryName = m_IStorage.GetFileName(path);
                var targetPath = m_IStorage.Combine(newTargetPath, targetdirectoryName);

                ProcessSnapBackupStep(path, targetPath);
            }

            BackupSessionHistory.SaveHistory(newTargetPath, m_BackupName, m_BackupSessionHistory);
        }


        public void ProcessSnapBackupStep(string sourcePath, string currSetPath)
        {
            var sourceFileList = m_IStorage.GetFiles(sourcePath);

            foreach (var file in sourceFileList)
            {
                UpdateProgress("Running... ", ++ProcessFileCount);

                HandleFile(sourcePath, currSetPath, file);
            }

            HandleDeletedFiles(sourceFileList, currSetPath);

            var sourceSubdirectoryEntriesList = GetDirectoriesNames(sourcePath);
            HandleDeletedItems(sourceSubdirectoryEntriesList, currSetPath);

            foreach (string subdirectory in sourceSubdirectoryEntriesList)
            {
                string newSourceSetPath = m_IStorage.Combine(sourcePath, subdirectory);
                string newCurrSetPath = m_IStorage.Combine(currSetPath, subdirectory);

                ProcessSnapBackupStep(newSourceSetPath, newCurrSetPath);
            }
        }
    }
}
