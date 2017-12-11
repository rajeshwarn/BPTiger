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
    class SelectHistoryItemsViewModel : BackupItemsTreeBase
    {
        //        public ICommand SaveFolderSelectionCommand { get; private set; } = new SaveFolderSelectionICommand<object>();

        public SelectHistoryItemsViewModel() : base()
        {
            //foreach (var item in ProjectData?.CurrentBackupProfile.BackupFolderList.Where(i => i.IsAvailable))
            //{
            //    SelectedItemList.Add(item);
            //}
        }

        private BackupSessionHistory m_SelectedHistoryItem;
        public BackupSessionHistory SelectedHistoryItem { get { return m_SelectedHistoryItem; }
            set {
                m_SelectedHistoryItem = value;
                InitItems();
                OnPropertyChanged();
            } }

        protected override FolderMenuItem CreateMenuItem(bool isFolder, bool isSelected, string path, string relativePath, string name, FolderMenuItem parentItem, FileAttributes attr, HistoryTypeEnum? historyType = null)
        {
            var menuItem = new BackupFolderMenuItem()
            {
                IsFolder = isFolder,
                HistoryType = historyType,
                Attributes = attr,
                Path = path,
                RelativePath = relativePath,
                Name = name,
                ParentItem = parentItem,
                Selected = isSelected,
            };



            return menuItem;
        }

        protected override HistoryTypeEnum? GetFolderHistoryType(string path)
        {
            return null;
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

            //var lastSetARchivePath = m_IStorage.Combine(lastSetPath, BackupProfileData.TargetBackupBaseDirectoryName);

            var historyItem = SelectedHistoryItem;
            if (historyItem == null)
            {
                return;
            }

            var lastSetPath = m_IStorage.Combine(profile.GetTargetBackupFolder(), m_LastSetPathCache);

            string targetPath = null;
            if (historyItem.HistoryData?.HistoryItemList.Count() > 0)
            {
                string path = historyItem.HistoryData?.HistoryItemList[0].TargetPath;
                string lastPath = path;
                while ((path != null) && (path != string.Empty) &&  (path != historyItem.HistoryData?.TargetPath))
                {
                    lastPath = path;
                    path = m_IStorage.GetDirectoryName(path);
                }

                if ((path != null) && (path != string.Empty))
                {
                    targetPath = m_IStorage.Combine(lastPath, BackupProfileData.TargetBackupBaseDirectoryName);
                }
            }

            
            foreach (var item in profile.BackupFolderList.Where(i => i.IsAvailable))
            {

                var directoryName = m_IStorage.GetFileName(item.Path);
                var restorePath = m_IStorage.Combine(lastSetPath, item.Name);

                var rootItem = CreateMenuItem(m_IStorage.IsFolder(restorePath), false, restorePath, directoryName, directoryName, null, 0);

                UpdateChildItemsInMenuItem(rootItem);

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    FolderMenuItemTree.Add(rootItem);
                }));
            }
        }
          

        protected override void AddFilesToFolderMenuItem(FolderMenuItem item, string itemPath, BackupSessionHistory history)
        {
            var profile = ProjectData.CurrentBackupProfile;
                ////History Items
                if (history != null)
                {
                    foreach (var historyItem in history.HistoryData?.HistoryItemList)
                    {
                        //var filePath = m_IStorage.Combine(item.Path, file);

                        if ((m_IStorage.GetDirectoryName(historyItem.TargetPath) == itemPath) ||
                            (historyItem.HistoryType == HistoryTypeEnum.Deleted && (m_IStorage.GetDirectoryName(historyItem.SourcePath) == itemPath)))
                        {
                            var fileName = m_IStorage.GetFileName(historyItem.TargetPath);
                            {
                                bool bSelected = false;
                                var rp = m_IStorage.Combine(item.RelativePath, fileName);

                                var foundItem = item.ChildFolderMenuItems.Where(i => i.Name == fileName).FirstOrDefault();
                                var timeDate = history?.HistoryData?.TimeStamp;
                                HistoryTypeEnum historyType = historyItem.HistoryType;

                                if (foundItem == null)
                                {
                                    var newItem = CreateMenuItem(historyItem.HistoryItemType == HistoryItemTypeEnum.Directory, bSelected, historyItem.TargetPath, rp, fileName, item, 0, historyType);
                                    Application.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        item.ChildFolderMenuItems.Add(newItem);
                                    }));

                                    foundItem = newItem;
                                }

                                var newMenuItem = CreateMenuItem(historyItem.HistoryItemType == HistoryItemTypeEnum.Directory, bSelected, historyItem.TargetPath, rp, timeDate.ToString(), foundItem, 0, historyType);

                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    foundItem.ChildFolderMenuItems.Add(newMenuItem);
                                }));
                            }
                        }
                    }
                }

            //Latest items
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
                        HistoryTypeEnum historyType = HistoryTypeEnum.NoChange;
                        bool bCreatedNew = false;
                        if (foundItem == null)
                        {
                            if (history?.HistoryData?.SessionHistoryIndex > 1)
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
                            bCreatedNew = true;
                        }

                        if (bCreatedNew || foundItem.ChildFolderMenuItems.Where(i => i.Path == file).FirstOrDefault() == null)
                        {
                            if (bDeletedItem)
                            {
                                historyType = HistoryTypeEnum.Deleted;
                            }
                            else
                            {
                                historyType = HistoryTypeEnum.Changed;
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
