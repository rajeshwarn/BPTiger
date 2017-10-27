using CompleteBackup.DataRepository;
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
    abstract class BackupItemsTreeBase : ObservableObject
    {
        public ICommand CloseWindowCommand { get; private set; } = new CloseWindowICommand<object>();

        public BackupProjectData ProjectData { get; set; } = BackupProjectRepository.Instance.SelectedBackupProject;
        public ObservableCollection<FolderMenuItem> FolderMenuItemTree { get; set; } = new ObservableCollection<FolderMenuItem>();
        public ObservableCollection<FolderData> SelectedItemList { get; set; } = new ObservableCollection<FolderData>();


        private bool m_DirtyFlag = false;
        public bool DirtyFlag { get { return m_DirtyFlag; } set { m_DirtyFlag = value; OnPropertyChanged(); } }

        protected IStorageInterface m_IStorage;


        public BackupItemsTreeBase()
        {
            m_IStorage = ProjectData.CurrentBackupProfile.GetStorageInterface();

            new Thread(new System.Threading.ThreadStart(() =>
            {
                AddRootItemsToTree();
            })) { IsBackground = true, Name = "Folder Tree Selection" }.Start();
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
        protected abstract FolderMenuItem CreateMenuItem(bool isFolder, bool isSelected, string path, string name, FolderMenuItem parentItem, FileAttributes attr);

        protected List<string> GetDirectoriesNames(string path)
        {
            //Process directories
            string[] sourceSubdirectoryEntries = m_IStorage.GetDirectories(path);
            var sourceSubdirectoryEntriesList = new List<string>();
            if (sourceSubdirectoryEntries != null)
            {
                if (sourceSubdirectoryEntriesList != null)
                {
                    foreach (var entry in sourceSubdirectoryEntries)
                    {
                        sourceSubdirectoryEntriesList.Add(m_IStorage.GetFileName(entry));
                    }
                }
            }

            return sourceSubdirectoryEntriesList;
        }

        private bool IsHidden(FileAttributes attr)
        {
            if (((attr & FileAttributes.System) != FileAttributes.System) &&
                ((attr & FileAttributes.Hidden) != FileAttributes.Hidden))
            {
                return false;
            }

            return true;
        }

        //Add and update all subitems
        protected void UpdateChildItemsInMenuItem(FolderMenuItem item)
        {
            if (item.IsFolder)
            {
                //Add all folders under item.Path
                var sourceSubdirectoryEntriesList = GetDirectoriesNames(item.Path);                
                foreach (string subdirectory in sourceSubdirectoryEntriesList)
                {
                    string newPath = m_IStorage.Combine(item.Path, subdirectory);
                    FileAttributes attr = m_IStorage.GetFileAttributes(newPath);
                    if (!IsHidden(attr))
                    {
                        bool bSelected = false;
                        if (item.Selected == true)
                        {
                            bSelected = true;
                        }

                        item.SourceBackupItems.Add(CreateMenuItem(true, bSelected, newPath, subdirectory, item, attr));
                    }
                }

                //Add all files under item.Path
                var fileList = m_IStorage.GetFiles(item.Path);
                foreach (var file in fileList)
                {
                    //var filePath = m_IStorage.Combine(item.Path, file);
                    var fileName = m_IStorage.GetFileName(file);
                    FileAttributes attr = File.GetAttributes(file);
                    if (!IsHidden(attr))
                    {
                        bool bSelected = false;
                        item.SourceBackupItems.Add(CreateMenuItem(false, bSelected, file, fileName, item, attr));
                    }
                }
            }
        }



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
            foreach (var folder in folderList.Where(i => (i.IsFolder)))
            {
                if (folder.Selected == true)
                {
                    SelectedItemList.Add(new FolderData { Path = folder.Path });
                }
                else if (folder.Selected == null)
                {
                    UpdateSelectedFolderListStep(profile, folder.SourceBackupItems);
                }
            }
        }

        void SelectItemDown(FolderMenuItem item)
        {
            if (item != null)
            {
                foreach (var subItem in item.SourceBackupItems)
                {
                    subItem.Selected = item.Selected;

                    //FolderList.Remove(subItem.Path);

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
                    foreach (var folder in parent.SourceBackupItems.Where(i => (i.IsFolder)))
                    {
                        if (folder.Selected != false)
                        {
                            bValue = null;
                            break;
                        }
                    }

                    parent.Selected = bValue;
                    SelectItemUp(parent);
                }
            }
        }


        public void ExpandFolder(ItemCollection itemList)
        {
            foreach (var item in itemList)
            {
                var folderItem = item as FolderMenuItem;
                if (folderItem.SourceBackupItems.Count() == 0)
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
