using CompleteBackup.DataRepository;
using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup.History;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Models.FolderSelection;
using CompleteBackup.ViewModels.FolderSelection.ICommands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CompleteBackup.ViewModels
{
    class SelectRestoreItemsWindowModel : BackupItemsTreeBase
    {

        enum RestoreActionTypeEnum
        {
            LatestVersion,
            SpecificFileVersion
        }

        RestoreActionTypeEnum RestoreActionType { get; set; }  = RestoreActionTypeEnum.SpecificFileVersion;

        public ICommand RestoreFolderSelectionCommand { get; private set; } = new RestoreFolderSelectionICommand<object>();

        protected string m_LastSetPathCache;

        public SelectRestoreItemsWindowModel() : base()
        {
        }

        protected override FolderMenuItem CreateMenuItem(bool isFolder, bool isSelected, string path, string relativePath, string name, FolderMenuItem parentItem, FileAttributes attr, HistoryTypeEnum? historyType = null)
        {
            var menuItem = new RestoreFolderMenuItem()
            {
                IsFolder = isFolder,
                HistoryType = historyType,
                Path = path,
                RelativePath = relativePath,
                Name = name,
                ParentItem = parentItem,
                Selected = isSelected,
            };

            return menuItem;
        }



        protected FolderMenuItem GetExistingMenuItem(bool isFolder, bool isSelected, string path, string relativePath, string name, FolderMenuItem parentItem, FileAttributes attr)
        {
            FolderMenuItem menuItem = null;
            foreach (var item in FolderMenuItemTree)
            {
                menuItem = GetMenuItemTree(item);
                if (menuItem != null)
                {
                    return menuItem;
                }
            }
            return menuItem;
        }

        FolderMenuItem  GetMenuItemTree(FolderMenuItem item)
        {
            if (item.RelativePath == item.RelativePath)
            {
                return item;
            }

            foreach (var childItem in item.ChildFolderMenuItems)
            {
                return GetMenuItemTree(childItem);
            }

            return null;
        }

        protected override HistoryTypeEnum? GetFolderHistoryType(string path)
        {
            HistoryTypeEnum? historyType = null;
            string folder = m_IStorage.Combine(m_IStorage.Combine(ProjectData.CurrentBackupProfile.TargetBackupFolder, m_LastSetPathCache), path);

            if (!m_IStorage.DirectoryExists(folder))
            {
                historyType = HistoryTypeEnum.Deleted;
            }

            return historyType;
        }


        public override void ExpandFolder(ItemCollection itemList)
        {
            if (RestoreActionType == RestoreActionTypeEnum.LatestVersion)
            {
                base.ExpandFolder(itemList);
            }
            else
            {
                var profile = ProjectData.CurrentBackupProfile;
                var backSetList = BackupManager.GetBackupSetList(profile);

                    //xxxxx
                foreach (var item in itemList)
                {
                    int iSessionIndex = 1;
                    foreach (var setPath in backSetList)
                    {
                        var sessionHistory = BackupSessionHistory.LoadHistory(profile.TargetBackupFolder, setPath);
                        sessionHistory.SessionHistoryIndex = iSessionIndex;
                        iSessionIndex++;

                        var folderItem = item as FolderMenuItem;

                        var lastSetPath = m_IStorage.Combine(profile.TargetBackupFolder, setPath);
                        var lastSetPath2 = m_IStorage.Combine(lastSetPath, folderItem.RelativePath);

                        //if (folderItem.ChildFolderMenuItems.Count() == 0)
                        {                            
                            UpdateChildItemsInMenuItem(folderItem, lastSetPath2, sessionHistory);
                        }
                    }
                }
            }
        }

        List<string> m_BackupSetPathCacheList = new List<string>();

        protected override void AddRootItemsToTree()
        {
            var profile = ProjectData.CurrentBackupProfile;
            ClearItemList();
            m_BackupSetPathCacheList.Clear();

            m_LastSetPathCache = BackupManager.GetLastBackupSetName(profile);

            switch (profile.BackupType)
            {
                case BackupTypeEnum.Snapshot:
                case BackupTypeEnum.Incremental:
                    {
                        var lastSetPath = m_IStorage.Combine(profile.TargetBackupFolder, m_LastSetPathCache);

                        foreach (var item in profile.BackupFolderList.Where(i => i.IsAvailable))
                        {
                            var directoryName = m_IStorage.GetFileName(item.Path);
                            var restorePath = m_IStorage.Combine(lastSetPath, directoryName);

                            var rootItem = CreateMenuItem(m_IStorage.IsFolder(restorePath), false, restorePath, directoryName, directoryName, null, 0);

                            UpdateChildItemsInMenuItem(rootItem);

                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                FolderMenuItemTree.Add(rootItem);
                            }));
                        }
                    }

                    break;
                case BackupTypeEnum.Differential:
                    {
                        var backSetList = BackupManager.GetBackupSetList(profile);
                        foreach (var setPath in backSetList.Where(p => p != m_LastSetPathCache))
                        {
                            m_BackupSetPathCacheList.Add(m_IStorage.Combine(profile.TargetBackupFolder, setPath));
                        }

                        m_RootFolderMenuItemTree.ParentItem = null;
                        m_RootFolderMenuItemTree.IsFolder = true;
                        m_RootFolderMenuItemTree.Path = profile.TargetBackupFolder;
                        m_RootFolderMenuItemTree.RelativePath = string.Empty;
                        m_RootFolderMenuItemTree.Name = "BACKUP";

                        int iSessionIndex = 0;
                        foreach (var setPath in backSetList)
                        {
                            iSessionIndex++;
                            var sessionHistory = BackupSessionHistory.LoadHistory(profile.TargetBackupFolder, setPath);
                            sessionHistory.SessionHistoryIndex = iSessionIndex;

                            var lastSetPath = m_IStorage.Combine(profile.TargetBackupFolder, setPath);

                            m_RootFolderMenuItemTree.Path = lastSetPath;

                            //update add root items
                            UpdateChildItemsInMenuItem(m_RootFolderMenuItemTree, lastSetPath, sessionHistory);

                            foreach (var subItem in m_RootFolderMenuItemTree.ChildFolderMenuItems)
                            {
                                var newPath = m_IStorage.Combine(lastSetPath, subItem.RelativePath);
                                UpdateChildItemsInMenuItem(subItem, newPath, sessionHistory);
                            }
                        }                        
                    }
                    break;

                default:
                    break;
            }           
        }

        protected override void AddFilesToFolderMenuItem(FolderMenuItem item, string itemPath, BackupSessionHistory history)
        {
            if (RestoreActionType == RestoreActionTypeEnum.LatestVersion)
            {
                var fileList = m_IStorage.GetFiles(itemPath);
                foreach (var file in fileList.Where(f => !IsPathExistsInPathList(f, item.ChildFolderMenuItems)))
                {
                    //var filePath = m_IStorage.Combine(item.Path, file);
                    var fileName = m_IStorage.GetFileName(file);
                    FileAttributes attr = File.GetAttributes(file);
                    if (!IsHidden(attr))
                    {
                        bool bSelected = false;
                        var rp = m_IStorage.Combine(item.RelativePath, fileName);
                        item.ChildFolderMenuItems.Add(CreateMenuItem(m_IStorage.IsFolder(file), bSelected, file, rp, fileName, item, attr));
                    }
                }
            }
            else
            {
                var fileList = m_IStorage.GetFiles(itemPath);
                foreach (var file in fileList)
                {
                    //var filePath = m_IStorage.Combine(item.Path, file);
                    var fileName = m_IStorage.GetFileName(file);
                    FileAttributes attr = File.GetAttributes(file);
                    if (!IsHidden(attr))
                    {
                        bool bSelected = false;
                        var rp = m_IStorage.Combine(item.RelativePath, fileName);

                        var foundItem = item.ChildFolderMenuItems.Where(i => i.Name == fileName).FirstOrDefault();
                        bool bDeletedItem = false;
                        HistoryTypeEnum historyType = HistoryTypeEnum.NoChange;
                        if (foundItem == null)
                        {
                            if (history?.SessionHistoryIndex > 1)
                            {
                                bDeletedItem = true;
                                historyType = HistoryTypeEnum.Deleted;
                            }

                            var newItem = CreateMenuItem(m_IStorage.IsFolder(file), bSelected, file, rp, fileName, item, attr, historyType);
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                item.ChildFolderMenuItems.Add(newItem);
                            }));

                            foundItem = newItem;
                        }

                        var timeDate = history?.TimeStamp;
                        
                        if (bDeletedItem)
                        {
                            historyType = HistoryTypeEnum.Deleted;
                        }
                        else
                        {
                            historyType = HistoryTypeEnum.Changed;
                        }
                        var newMenuItem = CreateMenuItem(m_IStorage.IsFolder(file), bSelected, file, rp, timeDate.ToString(), foundItem, attr, historyType);

                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            foundItem.ChildFolderMenuItems.Add(newMenuItem);
                        }));
                    }
                }
            }
        }
    }
}
