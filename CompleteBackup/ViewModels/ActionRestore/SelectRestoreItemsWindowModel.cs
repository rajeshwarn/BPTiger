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
using System.Diagnostics;
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
            string folder = m_IStorage.Combine(m_IStorage.Combine(ProjectData.CurrentBackupProfile.GetTargetBackupFolder(), m_LastSetPathCache), path);

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
                var backSetList = BackupBase.GetBackupSetList_(profile);

                    //xxxxx
                foreach (var item in itemList)
                {
                    int iSessionIndex = 1;
                    foreach (var setPath in backSetList)
                    {
                        var sessionHistory = BackupSessionHistory.LoadHistory(profile.GetTargetBackupFolder(), setPath);
                        if (sessionHistory != null)
                        {
                            sessionHistory.HistoryData.SessionHistoryIndex = iSessionIndex;
                            iSessionIndex++;

                            var folderItem = item as FolderMenuItem;

                            var lastSetPath = m_IStorage.Combine(profile.GetTargetBackupFolder(), m_IStorage.Combine(setPath, BackupProfileData.TargetBackupBaseDirectoryName));
                            var lastSetPath2 = m_IStorage.Combine(lastSetPath, folderItem.RelativePath);

                            //if (folderItem.ChildFolderMenuItems.Count() == 0)
                            {
                                UpdateChildItemsInMenuItem(folderItem, lastSetPath2, sessionHistory);
                            }
                        }
                    }
                }
            }
        }

        protected override void AddRootItemsToTree()
        {
            var profile = ProjectData.CurrentBackupProfile;
            ClearItemList();
            //m_BackupSetPathCacheList.Clear();

            m_LastSetPathCache = BackupBase.GetLastBackupSetPath_(profile);

            if (m_LastSetPathCache == null)
            {
                return;
            }

            switch (profile.BackupType)
            {
                case BackupTypeEnum.Snapshot:
                case BackupTypeEnum.Incremental:
                    {
                        var lastSetPath = m_IStorage.Combine(profile.GetTargetBackupFolder(), m_LastSetPathCache);

                        foreach (var item in profile.BackupFolderList.Where(i => i.IsAvailable))
                        {
                            
                            var directoryName = m_IStorage.GetFileName(item.Path);
                            var restorePath = m_IStorage.Combine(lastSetPath, item.Name);

                            if (m_IStorage.DirectoryExists(restorePath) || m_IStorage.FileExists(restorePath))
                            {
                                var rootItem = CreateMenuItem(m_IStorage.IsFolder(restorePath), false, restorePath, directoryName, directoryName, null, 0);

                                UpdateChildItemsInMenuItem(rootItem);

                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    FolderMenuItemTree.Add(rootItem);
                                }));
                            }
                            else
                            {
                                profile.Logger.Writeln($"Skipping restore item, Item not found: {restorePath}");
                            }
                        }
                    }

                    break;
                case BackupTypeEnum.Differential:
                    {
                        var backSetList = BackupBase.GetBackupSetList_(profile);
                        //var lastSetName = BackupBase.GetLastBackupSetPath_(profile);

                        //foreach (var setPath in backSetList.Where(p => p != m_LastSetPathCache))
                        //{
                        //    m_BackupSetPathCacheList.Add(m_IStorage.Combine(profile.GetTargetBackupFolder(), setPath));
                        //}

                        m_RootFolderMenuItemTree.ParentItem = null;
                        m_RootFolderMenuItemTree.IsFolder = true;
                        m_RootFolderMenuItemTree.Path = profile.GetTargetBackupFolder();
                        m_RootFolderMenuItemTree.RelativePath = string.Empty;
                        m_RootFolderMenuItemTree.Name = "BACKUP";

                        int iSessionIndex = 0;
                        foreach (var setPath in backSetList)
                        {
                            iSessionIndex++;
                            var sessionHistory = BackupSessionHistory.LoadHistory(profile.GetTargetBackupFolder(), setPath);
                            if (sessionHistory == null)
                            {
                                Trace.WriteLine("Error, Select Restore Differential Items, History is null");
                            }
                            else
                            {
                                sessionHistory.HistoryData.SessionHistoryIndex = iSessionIndex;

                                var lastSetPath = m_IStorage.Combine(m_IStorage.Combine(profile.GetTargetBackupFolder(), setPath), BackupProfileData.TargetBackupBaseDirectoryName);

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
                    }
                    break;

                default:
                    break;
            }           
        }

        protected override void AddFilesToFolderMenuItem(FolderMenuItem item, string itemPath, BackupSessionHistory history)
        {
            var profile = ProjectData.CurrentBackupProfile;

            if ((profile.BackupType != BackupTypeEnum.Differential) || (RestoreActionType == RestoreActionTypeEnum.LatestVersion))
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
                guyxxx build errro
                ////History Items
                //if (history != null)
                //{
                //    foreach (var historyItem in history.HistoryData?.HistoryItemList)
                //    {
                //        //var filePath = m_IStorage.Combine(item.Path, file);

                //        if ((m_IStorage.GetDirectoryName(historyItem.TargetPath) == itemPath) ||
                //            (historyItem.HistoryType == HistoryTypeEnum.Deleted && (m_IStorage.GetDirectoryName(historyItem.SourcePath) == itemPath)))
                //        {
                //            var fileName = m_IStorage.GetFileName(historyItem.TargetPath);
                //            {
                //                bool bSelected = false;
                //                var rp = m_IStorage.Combine(item.RelativePath, fileName);

                //                var foundItem = item.ChildFolderMenuItems.Where(i => i.Name == fileName).FirstOrDefault();
                //                var timeDate = history?.HistoryData?.TimeStamp;
                //                HistoryTypeEnum historyType = historyItem.HistoryType;

                //                if (foundItem == null)
                //                {
                //                    var newItem = CreateMenuItem(historyItem.HistoryItemType == HistoryItemTypeEnum.Directory, bSelected, historyItem.TargetPath, rp, fileName, item, 0, historyType);
                //                    Application.Current.Dispatcher.Invoke(new Action(() =>
                //                    {
                //                        item.ChildFolderMenuItems.Add(newItem);
                //                    }));

                //                    foundItem = newItem;
                //                }

                //                var newMenuItem = CreateMenuItem(historyItem.HistoryItemType == HistoryItemTypeEnum.Directory, bSelected, historyItem.TargetPath, rp, timeDate.ToString(), foundItem, 0, historyType);

                //                Application.Current.Dispatcher.Invoke(new Action(() =>
                //                {
                //                    foundItem.ChildFolderMenuItems.Add(newMenuItem);
                //                }));
                //            }
                //        }
                //    }
                //}


                if (m_IStorage.DirectoryExists(itemPath))
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
                            HistoryTypeEnum? historyType = null;// HistoryTypeEnum.NoChange;
                            bool bCreatedNew = false;
                            if (foundItem == null)
                            {
                                if (history?.HistoryData?.SessionHistoryIndex > 1)
                                {
                                    bDeletedItem = true;
 //                                   historyType = HistoryTypeEnum.Deleted;
                                }

                                var newItem = CreateMenuItem(m_IStorage.IsFolder(file), bSelected, file, rp, fileName, item, attr, historyType);
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    item.ChildFolderMenuItems.Add(newItem);
                                }));

                                foundItem = newItem;
                                bCreatedNew = true;
                            }

                            if (bCreatedNew || foundItem.ChildFolderMenuItems.Where(i => i.Path == file).FirstOrDefault() == null)
                            {
                                var foundItemInHistory = history.HistoryData.HistoryItemList.FirstOrDefault(i => i.TargetPath == rp);
                                if (foundItemInHistory != null)
                                {
                                    historyType = foundItemInHistory.HistoryType;
                                }

                                var timeDate = history?.HistoryData?.TimeStamp;
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
    }
}
