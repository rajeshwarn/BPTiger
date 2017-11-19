using CompleteBackup.DataRepository;
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
using System.Windows.Interop;

namespace CompleteBackup.ViewModels
{
    abstract class BackupItemsTreeBase : ObservableObject
    {
        public ICommand CloseWindowCommand { get; private set; } = new CloseWindowICommand<object>();

        public BackupProjectData ProjectData { get; set; } = BackupProjectRepository.Instance.SelectedBackupProject;


        protected FolderMenuItem m_RootFolderMenuItemTree = new FolderMenuItem();

        public ObservableCollection<FolderMenuItem> FolderMenuItemTree { get { return m_RootFolderMenuItemTree.ChildFolderMenuItems; } set { } }

        public ObservableCollection<FolderData> SelectedItemList { get; set; } = new ObservableCollection<FolderData>();




        private bool m_DirtyFlag = false;
        public bool DirtyFlag { get { return m_DirtyFlag; } set { m_DirtyFlag = value; OnPropertyChanged(); } }

        protected IStorageInterface m_IStorage;


        public BackupItemsTreeBase()
        {
            m_IStorage = ProjectData.CurrentBackupProfile.GetStorageInterface();

        }

        public void InitItems()
        {
            new Thread(new System.Threading.ThreadStart(() =>
            {
                AddRootItemsToTree();
            }))
            {
                IsBackground = true, Name = "Folder Tree Selection"
            }.Start();
        }

        protected virtual void ClearItemList()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                FolderMenuItemTree?.Clear();
            }));
        }

        protected virtual void ClearSelectedFolderList()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                SelectedItemList?.Clear();
            }));
        }

        protected abstract void AddRootItemsToTree();
        protected abstract FolderMenuItem CreateMenuItem(bool isFolder, bool isSelected, string path, string relativePath, string name, FolderMenuItem parentItem, FileAttributes attr, HistoryTypeEnum? historyType = null);


        protected bool IsHidden(FileAttributes attr)
        {
            if (((attr & FileAttributes.System) != FileAttributes.System) &&
                ((attr & FileAttributes.Hidden) != FileAttributes.Hidden))
            {
                return false;
            }

            return true;
        }

        protected abstract HistoryTypeEnum? GetFolderHistoryType(string path);

        protected bool IsNameExistsInNameList(string name, ObservableCollection<FolderMenuItem> list)
        {
            var item = list.Where(i => i.Name == name).FirstOrDefault();

            return item != null;
        }

        protected bool IsPathExistsInPathList(string path, ObservableCollection<FolderMenuItem> list)
        {
            var item = list.Where(i => i.Path == path).FirstOrDefault();

            return item != null;
        }


        //Add and update all subitems
        protected void UpdateChildItemsInMenuItem(FolderMenuItem item, string overridePath = null, BackupSessionHistory history = null)
        {
            var path = overridePath == null ? item.Path : overridePath;
            //Add all folders under item.Path
            if (item.IsFolder)// && (m_IStorage.DirectoryExists(path) || )
            {
                //Add folders
                AddFoldersToFolderMenuItem(item, overridePath, history);

                //Add files
                AddFilesToFolderMenuItem(item, path, history);
            }            
        }



        void AddFoldersToFolderMenuItem(FolderMenuItem item, string overridePath = null, BackupSessionHistory history = null)
        {
            var path = overridePath == null ? item.Path : overridePath;

            if (!m_IStorage.DirectoryExists(path))
            {
                return;
            }

            var subdirectoryList = m_IStorage.GetDirectoriesNames(path);
            foreach (string subdirectory in subdirectoryList.Where(i => !BackupSessionHistory.IsHistoryItem(i)))
            {
                string newPath = m_IStorage.Combine(path, subdirectory);
                FileAttributes attr = m_IStorage.GetFileAttributes(newPath);
                if (!IsHidden(attr) && !IsNameExistsInNameList(subdirectory, item.ChildFolderMenuItems))
                {
                    HistoryTypeEnum? historyType = GetFolderHistoryType(m_IStorage.Combine(item.RelativePath, subdirectory));

                    //if (history == null)// || history.SessionHistoryIndex == 1 || historyType == HistoryTypeEnum.Deleted)
                    {
                        bool bSelected = item.Selected == true;

                        var rp = m_IStorage.Combine(item.RelativePath, subdirectory);
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            item.ChildFolderMenuItems.Add(CreateMenuItem(m_IStorage.IsFolder(newPath), bSelected, newPath, rp, subdirectory, item, attr, historyType));
                        }));
                    }
                }
            }
        }


        protected abstract void AddFilesToFolderMenuItem(FolderMenuItem item, string itemPath, BackupSessionHistory history);

        //protected void AddFilesToFolderMenuItemBaseXXX(FolderMenuItem item, string itemPath, BackupSessionHistory history)
        //{
        //    var fileList = m_IStorage.GetFiles(itemPath);
        //    foreach (var file in fileList.Where(f => !IsPathExistsInPathList(f, item.ChildFolderMenuItems)))
        //    {
        //        //var filePath = m_IStorage.Combine(item.Path, file);
        //        var fileName = m_IStorage.GetFileName(file);
        //        FileAttributes attr = File.GetAttributes(file);
        //        if (!IsHidden(attr))
        //        {
        //            bool bSelected = false;
        //            var rp = m_IStorage.Combine(item.RelativePath, fileName);
        //            item.ChildFolderMenuItems.Add(CreateMenuItem(m_IStorage.IsFolder(file), bSelected, file, rp, fileName, item, attr));
        //        }
        //    }
        //}



        //----------

        protected void UpdateSelectedFolders(BackupProfileData profile, FolderMenuItem item)
        {
            SelectItemDown(item);
            SelectItemUp(item);

            UpdateSelectedFolderList(profile);
        }

        public void UpdateSelectedFolderList(BackupProfileData profile = null)
        {
            if (profile == null)
            {
                profile = ProjectData.CurrentBackupProfile;
            }

            //Update source folder selection list
            ClearSelectedFolderList();
            UpdateSelectedFolderListStep(profile, FolderMenuItemTree);

            //update folder properties/size in UI window
            //profile.UpdateProfileProperties();
        }
        void UpdateSelectedFolderListStep(BackupProfileData profile, ObservableCollection<FolderMenuItem> folderList)
        {
            foreach (var item in folderList.Where(i => (i.IsSelectable)))
            {
                if (item.Selected == true)
                {
                    SelectedItemList.Add(new FolderData { Path = item.Path, RelativePath = item.RelativePath, Name = item.Name, IsFolder = item.IsFolder });
                }
                else if (item.Selected == null)
                {
                    UpdateSelectedFolderListStep(profile, item.ChildFolderMenuItems);
                }
            }
        }

        void SelectItemDown(FolderMenuItem item)
        {
            if (item != null)
            {
                foreach (var subItem in item.ChildFolderMenuItems)
                {
                    subItem.Selected = item.Selected;
                    SelectItemDown(subItem);
                }
            }
        }
        void SelectItemUp(FolderMenuItem item)
        {
            if (item != null)
            {
                var parent = item.ParentItem;
                if (parent != null)
                {
                    bool? bValue = false;
                    foreach (var parentItem in parent.ChildFolderMenuItems)
                    {
                        if (parentItem.Selected != false)
                        {
                            bValue = null;
                            break;
                        }
                    }

                    parent.Selected = bValue;
                    SelectItemUp(parent);
                }

                item.IsExpanded = item.Selected == null;
            }
        }



        public virtual void ExpandFolder(ItemCollection itemList)
        {
            foreach (var item in itemList)
            {
                var folderItem = item as FolderMenuItem;
                if (folderItem.ChildFolderMenuItems.Count() == 0)
                {
                    UpdateChildItemsInMenuItem(folderItem);
                }
            }
        }


        public void FolderTreeClick(FolderMenuItem item, bool bSelected)
        {
            if (item != null)
            {
                DirtyFlag = true;
                item.Selected = bSelected;

                UpdateSelectedFolders(ProjectData.CurrentBackupProfile, item);
            }
        }
    }
}
