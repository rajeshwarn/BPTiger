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
    public class IncrementalBackup : BackupManager
    {
        public string LastSetPath;

        public IncrementalBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar)
        {
        }
//        public override string BackUpProfileSignature { get { return $"{BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile?.GUID.ToString("D")}-CBKP-INC"; } }

        public override void ProcessBackup()
        {
            //var backupProfileList = new List<string>();
            //string[] setEntries = m_IStorage.GetDirectories(TargetPath);
            //foreach (var entry in setEntries.Where(s => m_IStorage.GetFileName(s).StartsWith(BackUpProfileSignature)))
            //{
            //    backupProfileList.Add(m_IStorage.GetFileName(entry));
            //}

            //var lastSet = backupProfileList.OrderBy(set => set).LastOrDefault();

            var lastSet = BackupManager.GetLastBackupSetName(m_Profile);

            DateTime d = DateTime.Now;
            var targetSet = $"{m_Profile.BackupSignature}_{d.Year:0000}-{d.Month:00}-{d.Day:00}_{d.Hour:00}{d.Minute:00}{d.Hour:00}{d.Second:00}{d.Millisecond:000}";

            m_BackupSessionHistory.Clear();

            if (lastSet == null)
            {
                var newTargetPath = m_IStorage.Combine(TargetPath, targetSet);

                m_IStorage.CreateDirectory(newTargetPath);

                foreach (var item in SourcePath)
                {
                    var targetdirectoryName = m_IStorage.GetFileName(item.Path);
                    var targetPath = m_IStorage.Combine(newTargetPath, targetdirectoryName);

                    ProcessNewBackupStep(item.Path, targetPath);
                }

                BackupSessionHistory.SaveHistory(newTargetPath, targetSet, m_BackupSessionHistory);
            }
            else
            {

                var lastTargetPath_ = m_IStorage.Combine(TargetPath, lastSet);
                var newTargetPath = m_IStorage.Combine(TargetPath, targetSet);

                if (!m_IStorage.MoveDirectory(lastTargetPath_, newTargetPath))
                {
                    MessageBox.Show($"Operation Canceled", "Incremental Backup", MessageBoxButton.OK, MessageBoxImage.Information);

                    return;
                }

                m_IStorage.CreateDirectory(lastTargetPath_);

                var fileEntries = m_IStorage.GetFiles(newTargetPath);
                foreach (string fileName in fileEntries.Where(f => BackupSessionHistory.IsHistoryFile(f)))
                {
                    m_IStorage.MoveFile(m_IStorage.Combine(newTargetPath, m_IStorage.GetFileName(fileName)),
                                        m_IStorage.Combine(lastTargetPath_, m_IStorage.GetFileName(fileName)));
                }
               
                //check if set was changed and need to be deleted
                var prevSetList = m_IStorage.GetDirectories(newTargetPath);
                foreach (var path in prevSetList)
                {
                    var setName = m_IStorage.GetFileName(path);
                    var foundMatch = SourcePath.Where(f => m_IStorage.GetFileName(f.Path) == setName);
                    if (foundMatch.Count() == 0)
                    {
                        var sourcePath = m_IStorage.Combine(m_IStorage.GetDirectoryName(SourcePath.FirstOrDefault().Path), setName);

                        var targetPath = m_IStorage.Combine(newTargetPath, setName);
                        var lastTargetPath = m_IStorage.Combine(lastTargetPath_, setName);
                        m_IStorage.MoveDirectory(targetPath, lastTargetPath);

                        m_BackupSessionHistory.AddDeletedFolder(sourcePath);
                    }
                }

                foreach (var item in SourcePath)
                {
                    var targetdirectoryName = m_IStorage.GetFileName(item.Path);

                    var targetPath = m_IStorage.Combine(newTargetPath, targetdirectoryName);
                    var lastTargetPath = m_IStorage.Combine(lastTargetPath_, targetdirectoryName);

                    ProcessIncrementalStep(item.Path, targetPath, lastTargetPath);
                }

                BackupSessionHistory.SaveHistory(newTargetPath, targetSet, m_BackupSessionHistory);
            }
        }


        public void ProcessNewBackupStep(string sourcePath, string currSetPath)
        {
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

                m_BackupSessionHistory.AddNewFile(targetFilePath);
            }

            //Process directories
            var sourceSubdirectoryEntriesList = GetDirectoriesNames(sourcePath);

            foreach (string subdirectory in sourceSubdirectoryEntriesList)
            {
                string newSourceSetPath = m_IStorage.Combine(sourcePath, subdirectory);
                string newCurrSetPath = m_IStorage.Combine(currSetPath, subdirectory);

                ProcessNewBackupStep(newSourceSetPath, newCurrSetPath);
            }
        }
    

        public void ProcessIncrementalStep(string sourcePath, string currSetPath, string lastSetPath)
        {
            var sourceFileList = m_IStorage.GetFiles(sourcePath);

            m_IStorage.CreateDirectory(currSetPath, true);

            foreach (var file in sourceFileList)
            {
                UpdateProgress("Running... ", ++ProcessFileCount);

                var fileName = m_IStorage.GetFileName(file);
                HandleFile(sourcePath, currSetPath, lastSetPath, fileName);
            }

            HandleDeletedFiles(sourceFileList, currSetPath, lastSetPath);

            //Process directories
            var sourceSubdirectoryEntriesList = GetDirectoriesNames(sourcePath);

            HandleDeletedItems(sourceSubdirectoryEntriesList, currSetPath, lastSetPath);

            foreach (string subdirectory in sourceSubdirectoryEntriesList)
            {
                string newSourceSetPath = m_IStorage.Combine(sourcePath, subdirectory);
                string newCurrSetPath = m_IStorage.Combine(currSetPath, subdirectory);
                string newLastSetPath = m_IStorage.Combine(lastSetPath, subdirectory);

                ProcessIncrementalStep(newSourceSetPath, newCurrSetPath, newLastSetPath);
            }
        }
    }
}
