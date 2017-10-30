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
        bool RestoreLatestVersion = false;

        public ICommand RestoreFolderSelectionCommand { get; private set; } = new RestoreFolderSelectionICommand<object>();

        public SelectRestoreItemsWindowModel() : base()
        {
        }

        protected override FolderMenuItem CreateMenuItem(bool isFolder, bool isSelected, string path, string relativePath, string name, FolderMenuItem parentItem, FileAttributes attr)
        {
            var menuItem = new RestoreFolderMenuItem()
            {
                IsFolder = isFolder,
                Path = path,
                RelativePath = relativePath,
                Name = name,
                ParentItem = parentItem,
                Selected = isSelected,
                Image = GetImageSource(path),
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


            //menuItem = new RestoreFolderMenuItem()
            //{
            //    IsFolder = isFolder,
            //    Path = path,
            //    RelativePath = relativePath,
            //    Name = name,
            //    ParentItem = parentItem,
            //    Selected = isSelected,
            //    Image = GetImageSource(path),
            //};

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


        public override void ExpandFolder(ItemCollection itemList)
        {
            if (RestoreLatestVersion)
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
                    foreach (var setPath in backSetList)
                    {
                        var folderItem = item as FolderMenuItem;

                        var lastSetPath = m_IStorage.Combine(profile.TargetBackupFolder, setPath);
                        var lastSetPath2 = m_IStorage.Combine(lastSetPath, folderItem.RelativePath);

                        //if (folderItem.ChildFolderMenuItems.Count() == 0)
                        {
                            UpdateChildItemsInMenuItem(folderItem, lastSetPath2);
                        }
                    }
                }
            }
        }

        List<string> m_BackupSetPathList = new List<string>();

        bool bLoadFromHistory = false;
        protected override void AddRootItemsToTree()
        {
            var profile = ProjectData.CurrentBackupProfile;
            ClearItemList();
            m_BackupSetPathList.Clear();

            var lastSet = BackupManager.GetLastBackupSetName(profile);
            var backSetList = BackupManager.GetBackupSetList(profile);
            foreach(var setPath in backSetList.Where(p => p != lastSet))
            {
                m_BackupSetPathList.Add(m_IStorage.Combine(profile.TargetBackupFolder, setPath));
            }

            if (bLoadFromHistory)
            {
                AddRootItemsToTreeFromHistory();
            }
            else
            {
                bool bIncremental = true;

                if (bIncremental)
                {
                    foreach (var setPath in backSetList)
                    {
                        var lastSetPath = m_IStorage.Combine(profile.TargetBackupFolder, setPath);

                        foreach (var item in profile.FolderList)
                        {
                            var directoryName = m_IStorage.GetFileName(item.Path);
                            var restorePath = m_IStorage.Combine(lastSetPath, directoryName);

                            var rootItem = GetExistingMenuItem(true, false, restorePath, directoryName, directoryName, null, 0);
                            if (rootItem == null)
                            {
                                rootItem = CreateMenuItem(m_IStorage.IsFolder(restorePath), false, restorePath, directoryName, directoryName, null, 0);

                                UpdateChildItemsInMenuItem(rootItem);

                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    FolderMenuItemTree.Add(rootItem);
                                }));
                            }
                            else
                            {
                                UpdateChildItemsInMenuItem(rootItem);
                            }
                        }
                    }
                }
                else
                {
                    var lastSetPath = m_IStorage.Combine(profile.TargetBackupFolder, lastSet);

                    foreach (var item in profile.FolderList)
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
            }
        }

        protected override void AddFilesToFolderMenuItem(FolderMenuItem item, string itemPath)
        {
            if (RestoreLatestVersion)
            {
                var fileList = m_IStorage.GetFiles(itemPath);
                foreach (var file in fileList.Where(f => NoTExistsinTreeList(f, item.ChildFolderMenuItems)))
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
//                foreach (var file in fileList.Where(f => NoTExistsinTreeList(f, item.ChildFolderMenuItems)))
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
                        if (foundItem == null)
                        {
                            var newItem = CreateMenuItem(m_IStorage.IsFolder(file), bSelected, file, rp, fileName, item, attr);
                            item.ChildFolderMenuItems.Add(newItem);

                            foundItem = newItem;
                        }


                        foundItem.ChildFolderMenuItems.Add(CreateMenuItem(m_IStorage.IsFolder(file), bSelected, file, rp, fileName, item, attr));
                    }
                }
            }
        }



        protected override List<string> GetAllActiveSets(FolderMenuItem item)
        {
            var activeSetList = new List<string>() { item.Path };

            foreach(var set in m_BackupSetPathList)
            {
                activeSetList.Add(m_IStorage.Combine(set, item.Name));
            }

            return activeSetList;
        }




















        /// <summary>
        /// Load from history file
        /// </summary>
        void AddRootItemsToTreeFromHistory()
        {
            var profile = ProjectData.CurrentBackupProfile;

            ClearItemList();

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var setList = BackupManager.GetBackupSetList(profile);
                foreach (var set in setList)
                {
                    var sessionHistory = BackupSessionHistory.LoadHistory(profile.TargetBackupFolder, set);

                    if (set == setList[0])
                    {
                        //last set add all items
                        foreach (var item in sessionHistory.HistoryItemList)
                        {
                            InsertNamesToTreeFromHistory(sessionHistory, item, item.SourcePath, 0);
                        }
                    }
                    else
                    {
                        foreach (var item in sessionHistory.HistoryItemList.Where(i => i.HistoryType != HistoryTypeEnum.NoChange))
                        {
                            InsertNamesToTreeFromHistory(sessionHistory, item, item.SourcePath, 0);
                        }
                    }
                }
            }));
        }

        FolderMenuItem InsertNamesToTreeFromHistory(BackupSessionHistory history, HistoryItem item, string path, int iCount)
        {
            if (path != null)
            {
                var name = m_IStorage.GetFileName(path);
                if ((name != null) && (name != string.Empty))
                {

                    var newPath = path.Substring(0, path.Length - name.Length - 1);

                    var menuItem = InsertNamesToTreeFromHistory(history, item, newPath, iCount + 1);
                    var newMenuItem = menuItem.ChildFolderMenuItems.Where(m => m.Name == name).FirstOrDefault();
                    if (newMenuItem == null)
                    {
                        newMenuItem = new RestoreFolderMenuItem() { IsFolder = true, Path = path, Name = name };
                        menuItem.ChildFolderMenuItems.Add(newMenuItem);
                    }

                    if (iCount == 0)
                    {
                        var setTimeMenuItem = new RestoreFolderMenuItem() { Name = history.TimeStamp.ToString(), IsFolder = true, HistoryType = item.HistoryType };
                        newMenuItem.ChildFolderMenuItems.Add(setTimeMenuItem);
                    }

                    return newMenuItem;
                }
                else
                {
                    name = path;

                    FolderMenuItem menuItem = FolderMenuItemTree.Where(m => m.Path == name).FirstOrDefault();
                    if (menuItem == null)
                    {
                        menuItem = new RestoreFolderMenuItem() { IsFolder = true, Path = path, Name = name };
                        FolderMenuItemTree.Add(menuItem);
                    }

                    return menuItem;
                }
            }

            return null;
        }
    }
}
