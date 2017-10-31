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


        protected string m_LastSetPathCache;

        private bool m_DirtyFlag = false;
        public bool DirtyFlag { get { return m_DirtyFlag; } set { m_DirtyFlag = value; OnPropertyChanged(); } }

        protected IStorageInterface m_IStorage;

        protected System.Windows.Media.ImageSource GetImageSource(string path)
        {
            System.Windows.Media.ImageSource imageSource = null;
            try
            {
                var icon = m_IStorage.ExtractIconFromPath(path);

                imageSource = Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    System.Windows.Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to get Icon {path}\n{ex.Message}");
            }

            return imageSource;
        }


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
        protected abstract FolderMenuItem CreateMenuItem(bool isFolder, bool isSelected, string path, string relativePath, string name, FolderMenuItem parentItem, FileAttributes attr, HistoryTypeEnum? historyType = null);

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

        protected bool IsHidden(FileAttributes attr)
        {
            if (((attr & FileAttributes.System) != FileAttributes.System) &&
                ((attr & FileAttributes.Hidden) != FileAttributes.Hidden))
            {
                return false;
            }

            return true;
        }

        protected abstract List<string> GetAllActiveSets(FolderMenuItem item);
        protected abstract bool IsDeletedFolder(string path);

        protected bool NoTExistsinTreeNameList(string name, ObservableCollection<FolderMenuItem> list)
        {
            var item = list.Where(i => i.Name == name).FirstOrDefault();

            return item == null;
        }
        protected bool NoTExistsinTreeList(string path, ObservableCollection<FolderMenuItem> list)
        {
            var item = list.Where(i => i.Path == path).FirstOrDefault();

            return item == null;
        }



        //Add and update all subitems
        protected void UpdateChildItemsInMenuItem(FolderMenuItem item, string overridePath = null, BackupSessionHistory history = null)
        {
            if (item.IsFolder)
            {
                //Add all folders under item.Path
                if (m_IStorage.DirectoryExists(item.Path))
                {
                    var sourceSubdirectoryEntriesList = GetDirectoriesNames(item.Path);
                    foreach (string subdirectory in sourceSubdirectoryEntriesList)
                    {
                        string newPath = m_IStorage.Combine(item.Path, subdirectory);
                        FileAttributes attr = m_IStorage.GetFileAttributes(newPath);
                        if (!IsHidden(attr) && NoTExistsinTreeNameList(subdirectory, item.ChildFolderMenuItems))
                        {
                            HistoryTypeEnum? historyType = null;
                            bool bDeletedFolder = IsDeletedFolder(m_IStorage.Combine(item.RelativePath, subdirectory));
                            if (bDeletedFolder)
                            {
                                historyType = HistoryTypeEnum.Deleted;
                            }

                            if (history == null || history.SessionHistoryIndex == 1 || bDeletedFolder)
                            {
                                bool bSelected = false;
                                if (item.Selected == true)
                                {
                                    bSelected = true;
                                }

                                var rp = m_IStorage.Combine(item.RelativePath, subdirectory);
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    item.ChildFolderMenuItems.Add(CreateMenuItem(m_IStorage.IsFolder(newPath), bSelected, newPath, rp, subdirectory, item, attr, historyType));
                                }));
                            }
                        }
                    }

                    //Add all files under item.Path
                    string itemPath = item.Path;
                    if (overridePath != null)
                    {
                        itemPath = overridePath;
                        if (!m_IStorage.DirectoryExists(itemPath))
                        {
                            return;
                        }
                    }


                    //Add files to Item
                    AddFilesToFolderMenuItem(item, itemPath, history);

                    //var fileList = m_IStorage.GetFiles(itemPath);
                    //foreach (var file in fileList.Where(f => NoTExistsinTreeList(f, item.ChildFolderMenuItems)))
                    //{
                    //    //var filePath = m_IStorage.Combine(item.Path, file);
                    //    var fileName = m_IStorage.GetFileName(file);
                    //    FileAttributes attr = File.GetAttributes(file);
                    //    if (!IsHidden(attr))
                    //    {
                    //        bool bSelected = false;
                    //        var rp = m_IStorage.Combine(item.RelativePath, fileName);
                    //        item.ChildFolderMenuItems.Add(CreateMenuItem(m_IStorage.IsFolder(file), bSelected, file, rp, fileName, item, attr));
                    //    }
                    //}

                }
            }
        }

        protected abstract void AddFilesToFolderMenuItem(FolderMenuItem item, string itemPath, BackupSessionHistory history);


        protected void AddFilesToFolderMenuItemBase(FolderMenuItem item, string itemPath, BackupSessionHistory history)
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


        //protected void UpdateChildItemsInMenuItem(FolderMenuItem item)
        //{
        //    if (item.IsFolder)
        //    {
        //        var pathList = GetAllActiveSets(item);

        //        int i = 0;
        //        foreach (var path in pathList)
        //        {
        //            i++;
        //            //Add all folders under item.Path
        //            if (m_IStorage.DirectoryExists(path))
        //            {
        //                if (i > 1)
        //                {
        //                    int tttt = 0;
        //                }

        //                var sourceSubdirectoryEntriesList = GetDirectoriesNames(path);
        //                foreach (string subdirectory in sourceSubdirectoryEntriesList)
        //                {
        //                    string newPath = m_IStorage.Combine(item.Path, subdirectory);
        //                    FileAttributes attr = m_IStorage.GetFileAttributes(newPath);
        //                    if (!IsHidden(attr) && NoTExistsinTreeList(newPath, item.SourceBackupItems))
        //                    {
        //                        bool bSelected = false;
        //                        if (item.Selected == true)
        //                        {
        //                            bSelected = true;
        //                        }

        //                        var rp = m_IStorage.Combine(item.RelativePath, subdirectory);
        //                        item.SourceBackupItems.Add(CreateMenuItem(true, bSelected, newPath, rp, subdirectory, item, attr));
        //                    }
        //                }

        //                //Add all files under item.Path
        //                var fileList = m_IStorage.GetFiles(path);
        //                foreach (var file in fileList.Where(f => NoTExistsinTreeList(f, item.SourceBackupItems)))
        //                {
        //                    //var filePath = m_IStorage.Combine(item.Path, file);
        //                    var fileName = m_IStorage.GetFileName(file);
        //                    FileAttributes attr = File.GetAttributes(file);
        //                    if (!IsHidden(attr))
        //                    {
        //                        //                                find if item already added
        //                        //                              var eItem = item.SourceBackupItems.FirstOrDefault(it => !it.IsFolder && it.Name == fileName);
        //                        FolderMenuItem eItem = null;
        //                        if (eItem == null)
        //                        {
        //                            var rpeItemPath = m_IStorage.Combine(item.RelativePath, fileName);
        //                            var repItem = CreateMenuItem(false, false, file, rpeItemPath, fileName, item, attr);
        //                            item.SourceBackupItems.Add(repItem);

        //                            eItem = repItem;
        //                        }

        //                        //  bool bSelected = false;
        //                        //    var rp = m_IStorage.Combine(item.RelativePath, fileName);
        //                        //  eItem.SourceBackupItems.Add(CreateMenuItem(false, bSelected, file, rp, "SIG", eItem, attr));

        //                        //bool bSelected = false;
        //                        //var rp = m_IStorage.Combine(item.RelativePath, fileName);
        //                        //item.SourceBackupItems.Add(CreateMenuItem(false, bSelected, file, rp, fileName, item, attr));
        //                    }
        //                }
        //            }
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
