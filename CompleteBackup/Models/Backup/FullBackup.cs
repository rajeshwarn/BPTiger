using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.History;
using CompleteBackup.Models.Backup.Profile;
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
    public class FullBackup : BackupManager
    {
        public FullBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar)
        {
            m_IStorage = new FileSystemStorage();
        }
        //string m_BackupName;

        public override void ProcessBackup()
        {
            m_BackupSessionHistory.Reset(GetTimeStamp());
            string backupName = GetTargetSetName();

            var newTargetPath = m_IStorage.Combine(TargetPath, backupName);
            m_IStorage.CreateDirectory(newTargetPath);

            foreach (var item in SourcePath)
            {
                if (item.IsFolder)
                {
                    var targetPath = m_IStorage.Combine(newTargetPath, m_IStorage.GetFileName(item.Path));
                    ProcessFullBackupFolderStep(item.Path, targetPath);
                }
                else
                {
                    ProcessFullBackupFile(item.Path, m_IStorage.GetDirectoryName(item.Path), newTargetPath);
                }
            }

            BackupSessionHistory.SaveHistory(TargetPath, backupName, m_BackupSessionHistory);
        }


        public void ProcessFullBackupFile(string file, string sourcePath, string destPath)
        {
            UpdateProgress("Running... ", ++ProcessFileCount);

            var fileName = m_IStorage.GetFileName(file);
            // first set, copy to new set
            var sourceFilePath = m_IStorage.Combine(sourcePath, fileName);
            var targetFilePath = m_IStorage.Combine(destPath, fileName);

            m_IStorage.CopyFile(sourceFilePath, targetFilePath);

            m_BackupSessionHistory.AddNewFile(sourceFilePath, targetFilePath);
        }

        public void ProcessFullBackupFolderStep(string sourcePath, string currSetPath)
        {
            var sourceFileList = m_IStorage.GetFiles(sourcePath);
            m_IStorage.CreateDirectory(currSetPath);

            foreach (var file in sourceFileList)
            {
                ProcessFullBackupFile(file, sourcePath, currSetPath);
            }

            //Process directories
            var sourceSubdirectoryEntriesList = GetDirectoriesNames(sourcePath);

            foreach (string subdirectory in sourceSubdirectoryEntriesList)
            {
                string newSourceSetPath = m_IStorage.Combine(sourcePath, subdirectory);
                string newCurrSetPath = m_IStorage.Combine(currSetPath, subdirectory);

                ProcessFullBackupFolderStep(newSourceSetPath, newCurrSetPath);
            }
        }
    }
}
