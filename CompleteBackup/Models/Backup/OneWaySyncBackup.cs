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
    public class OneWaySyncBackup : BackupManager
    {
        public OneWaySyncBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar)
        {
            m_IStorage = new FileSystemStorage();
        }
//        public override string BackUpProfileSignature { get { return $"{BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile?.GUID.ToString("D")}-CBKP-SNAP"; } }
        string m_BackupName;

        public override void ProcessBackup()
        {
            if (m_BackupName == null)
            {
                //TimeStamp = DateTime.Now;
                m_BackupName = GetTargetSetName();// $"{m_Profile.BackupSignature}_{GetTimeStampString()}";
            }

            var newTargetPath = m_IStorage.Combine(TargetPath, m_BackupName);
    
            m_IStorage.CreateDirectory(newTargetPath);

            m_BackupSessionHistory.Reset(GetTimeStamp());


            foreach (var item in SourcePath)
            {
                var targetdirectoryName = m_IStorage.GetFileName(item.Path);
                var targetPath = m_IStorage.Combine(newTargetPath, targetdirectoryName);

                if (item.IsFolder)
                {
                    ProcessSnapBackupStep(item.Path, targetPath);
                }
                else
                {
                    UpdateProgress("Running... ", ++ProcessFileCount);

                    var fileName = m_IStorage.GetFileName(item.Path);
                    // first set, copy to new set
                    var targetFilePath = m_IStorage.Combine(newTargetPath, fileName);

                    m_IStorage.CopyFile(item.Path, targetFilePath);

                    m_BackupSessionHistory.AddNewFile(item.Path, targetFilePath);
                }
            }

            //foreach (var item in SourcePath)
            //{
            //    var targetdirectoryName = m_IStorage.GetFileName(item.Path);
            //    var targetPath = m_IStorage.Combine(newTargetPath, targetdirectoryName);

            //    ProcessSnapBackupStep(item.Path, targetPath);
            //}

            BackupSessionHistory.SaveHistory(TargetPath, m_BackupName, m_BackupSessionHistory);
        }


        public void ProcessSnapBackupStep(string sourcePath, string currSetPath)
        {
            //            var sourceFileList = m_IStorage.GetFiles(sourcePath);

            var sourceFileList = m_IStorage.GetFiles(sourcePath);

            m_IStorage.CreateDirectory(currSetPath);

            foreach (var file in sourceFileList)
            {
                UpdateProgress("Running... ", ++ProcessFileCount);

                var fileName = m_IStorage.GetFileName(file);
                // first set, copy to new set
                var sourceFilePath = m_IStorage.Combine(sourcePath, fileName);
                var targetFilePath = m_IStorage.Combine(currSetPath, fileName);

                m_IStorage.CopyFile(sourceFilePath, targetFilePath);

                m_BackupSessionHistory.AddNewFile(sourceFilePath, targetFilePath);
            }

            //Process directories
            var sourceSubdirectoryEntriesList = GetDirectoriesNames(sourcePath);

            foreach (string subdirectory in sourceSubdirectoryEntriesList)
            {
                string newSourceSetPath = m_IStorage.Combine(sourcePath, subdirectory);
                string newCurrSetPath = m_IStorage.Combine(currSetPath, subdirectory);

                ProcessSnapBackupStep(newSourceSetPath, newCurrSetPath);
            }



            //    UpdateProgress("Running... ", ++ProcessFileCount);

            //    HandleFile(sourcePath, currSetPath, file);
            //}


            //HandleDeletedFiles(sourceFileList, currSetPath);

            //var sourceSubdirectoryEntriesList = GetDirectoriesNames(sourcePath);
            //HandleDeletedItems(sourceSubdirectoryEntriesList, currSetPath);

            //foreach (string subdirectory in sourceSubdirectoryEntriesList)
            //{
            //    string newSourceSetPath = m_IStorage.Combine(sourcePath, subdirectory);
            //    string newCurrSetPath = m_IStorage.Combine(currSetPath, subdirectory);

            //    ProcessSnapBackupStep(newSourceSetPath, newCurrSetPath);
            //}
        }
    }
}
