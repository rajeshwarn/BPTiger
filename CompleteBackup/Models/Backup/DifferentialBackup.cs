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
    public class DifferentialBackup : SnapshotBackup
    {
        public string LastSetPath;

        public DifferentialBackup(BackupProfileData profile, GenericStatusBarView progressBar = null) : base(profile, progressBar) { }

        public override void ProcessBackup()
        {
            var lastSet = BackupManager.GetLastBackupSetName(m_Profile);
            var targetSet = GetTargetSetName();

            m_BackupSessionHistory.Reset(GetTimeStamp());

            if (lastSet == null)
            {
                string backupName = GetTargetSetName();
                string targetSetPath = CreateNewBackupSetFolder(backupName);

                ProcessBackupRootFolders(targetSetPath);

                BackupSessionHistory.SaveHistory(m_TargetBackupPath, targetSet, m_BackupSessionHistory);
            }
            else
            {

                var lastTargetPath_ = m_IStorage.Combine(m_TargetBackupPath, lastSet);
                var newTargetPath = m_IStorage.Combine(m_TargetBackupPath, targetSet);

                if (!MoveDirectory(lastTargetPath_, newTargetPath))
                {
                    MessageBox.Show($"Operation Canceled", "Incremental Backup", MessageBoxButton.OK, MessageBoxImage.Information);

                    return;
                }

                CreateDirectory(lastTargetPath_);

                var fileEntries = m_IStorage.GetFiles(newTargetPath);
                foreach (string fileName in fileEntries.Where(f => BackupSessionHistory.IsHistoryFile(f)))
                {
                    MoveFile(m_IStorage.Combine(newTargetPath, m_IStorage.GetFileName(fileName)),
                             m_IStorage.Combine(lastTargetPath_, m_IStorage.GetFileName(fileName)));
                }
               
                //check if set was changed and need to be deleted
                var prevSetList = m_IStorage.GetDirectories(newTargetPath);
                foreach (var path in prevSetList)
                {
                    var setName = m_IStorage.GetFileName(path);
                    var foundMatch = m_SourceBackupPathList.Where(f => m_IStorage.GetFileName(f.Path) == setName);
                    if (foundMatch.Count() == 0)
                    {
                        var sourcePath = m_IStorage.Combine(m_IStorage.GetDirectoryName(m_SourceBackupPathList.FirstOrDefault().Path), setName);

                        var targetPath = m_IStorage.Combine(newTargetPath, setName);
                        var lastTargetPath = m_IStorage.Combine(lastTargetPath_, setName);
                        MoveDirectory(targetPath, lastTargetPath);

                        m_BackupSessionHistory.AddDeletedFolder(sourcePath, newTargetPath);
                    }
                }

                foreach (var item in m_SourceBackupPathList)
                {
                    var targetdirectoryName = m_IStorage.GetFileName(item.Path);

                    var targetPath = m_IStorage.Combine(newTargetPath, targetdirectoryName);
                    var lastTargetPath = m_IStorage.Combine(lastTargetPath_, targetdirectoryName);

                    if (item.IsFolder)
                    {
                        ProcessIncrementalStep(item.Path, targetPath, lastTargetPath);
                    }
                    else
                    {
                        UpdateProgress("Running... ", ++ProcessFileCount, item.Path);
                        HandleFile(m_IStorage.GetDirectoryName(item.Path), newTargetPath, lastTargetPath, targetdirectoryName);
                    }
                }

                BackupSessionHistory.SaveHistory(m_TargetBackupPath, targetSet, m_BackupSessionHistory);
            }
        }

        public void ProcessIncrementalStep(string sourcePath, string currSetPath, string lastSetPath)
        {
            var sourceFileList = m_IStorage.GetFiles(sourcePath);

            m_IStorage.CreateDirectory(currSetPath, true);

            foreach (var file in sourceFileList)
            {
                var fileName = m_IStorage.GetFileName(file);
                UpdateProgress("Running... ", ++ProcessFileCount, file);

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
