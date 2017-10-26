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
    class ChangeBackupItemsWindowModel : ObservableObject
    {
        public ICommand SaveFolderSelectionCommand { get; private set; } = new SaveFolderSelectionICommand<object>();
        public ICommand CloseWindowCommand { get; private set; } = new CloseWindowICommand<object>();

        public BackupProjectData ProjectData { get; set; } = BackupProjectRepository.Instance.SelectedBackupProject;

        private bool m_DirtyFlag = false;
        public bool DirtyFlag { get { return m_DirtyFlag; } set { m_DirtyFlag = value; OnPropertyChanged(); } }

        public ChangeBackupItemsWindowModel()
        {
            new Thread(new System.Threading.ThreadStart(() =>
            {
                RefreshRootFolders();
            }))
            {
                IsBackground = true,
                Name = "Source folder reader"
            }.Start();
        }


        private void RefreshRootFolders()
        {
            var ProfileData = ProjectData.CurrentBackupProfile;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ProfileData.RootFolderItemList.Clear();
            }));

            DriveInfo[] drives = DriveInfo.GetDrives();

            //Add all available drives to list
            foreach (var drive in drives)
            {
                DirectoryInfo dInfo = drive.RootDirectory;
                try
                {
                    FileAttributes attr = File.GetAttributes(drive.Name);
                    var rootItem = new FolderMenuItem()
                    {
                        IsFolder = true,
                        Attributes = attr,
                        Path = drive.Name,
                        Name = $"{drive.VolumeLabel} ({drive.DriveType}) ({drive.Name})"
                    };

                    AddFoldersToTree(rootItem);

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        ProfileData.RootFolderItemList.Add(rootItem);
                    }));
                }
                catch
                {
                }
            }

            //Add selected folders to tree list
            var itemList = new List<FolderMenuItem>();
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var folder in ProfileData.FolderList)
                {
                    string pr = Directory.GetDirectoryRoot(folder.Path);
                    var match = ProfileData.RootFolderItemList.Where(f => String.Compare(f.Path, pr, true) == 0);

                    if (match.Count() > 0)
                    {
                        var item = AddFolderName(match.ElementAt(0), folder.Path);
                        itemList.Add(item);
                    }
                }
            }));

            //After we added the folders, we need update selected folders root items to mark the correct selection
            foreach (var item in itemList)
            {
                item.Selected = true;
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    UpdateSelectedFolders(ProfileData, item);
                }));
            }
        }
    


        IStorageInterface m_IStorage = new FileSystemStorage();

        FolderMenuItem AddFolderName(FolderMenuItem item, string path)
        {
            string pr = Directory.GetDirectoryRoot(path);
            var parentPath = Directory.GetParent(path);

            if (parentPath != null)
            {
                string name = parentPath.FullName;
                var parent = AddFolderName(item, name);

                var found = parent.SourceBackupItems.Where(i => String.Compare(i.Path, path, true) == 0);
                if (found.Count() == 0)
                {
                    FileAttributes attr = File.GetAttributes(path);
                    var newItem = new FolderMenuItem() { IsFolder = true, Attributes = attr, Path = path, Name = path, ParentItem = parent, Selected = false };
                    parent.SourceBackupItems.Add(newItem);
                    return newItem;
                }
                else
                {
                    AddFoldersToTree(found.ElementAt(0));
                    return found.ElementAt(0);
                }

            }
            else
            {
                return item;
            }
        }

        void AddFoldersToTree(FolderMenuItem item)
        {
            if (item.IsFolder)
            {
                var sourceSubdirectoryEntriesList = GetDirectoriesNames(item.Path);
                foreach (string subdirectory in sourceSubdirectoryEntriesList)
                {
                    string newPath = m_IStorage.Combine(item.Path, subdirectory);


                    //MOVE THIS TO FS class
                    FileAttributes attr = 0;
                    if (newPath.Length < Win32LongPathFile.MAX_PATH)
                    {
                        attr = File.GetAttributes(newPath);
                    }
                    else
                    {
                        attr = (FileAttributes)Win32FileSystem.GetFileAttributesW(Win32LongPathFile.GetWin32LongPath(newPath));
                    }

                    if (((attr & FileAttributes.System) != FileAttributes.System) &&
                        ((attr & FileAttributes.Hidden) != FileAttributes.Hidden))
                    {
                        var newItem = new FolderMenuItem() { IsFolder = true, Attributes = attr, Path = newPath, Name = subdirectory, ParentItem = item, Selected = false };
                        item.SourceBackupItems.Add(newItem);

                        //var match = ProjectData.FolderList.Where(f => String.Compare(f, newPath, true) == 0);

                        //var iCount = match.Count();
                        //if (iCount > 0)
                        //{
                        //    newItem.Selected = true;
                        //}
                        //else
                        //{
                        if (item.Selected == true)
                        {
                            newItem.Selected = true;
                        }
                        else
                        {
                            newItem.Selected = false;
                        }
                        //     }

                        //SelectItemUp(newItem);

                        //else if ((match != null) && (match.Count() > 0))
                        //{
                        //    SelectItemUp(newItem, true);
                        //}
                    }
                }

                var fileList = m_IStorage.GetFiles(item.Path);
                foreach (var file in fileList)
                {
                    var filePath = m_IStorage.Combine(item.Path, file);
                    FileAttributes attr = File.GetAttributes(filePath);

                    item.SourceBackupItems.Add(new FolderMenuItem() { IsFolder = false, Attributes = attr, Name = file, Path = filePath, Image = null });
                }
            }
        }

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


        public void FolderTreeClick(FolderMenuItem item, bool bSelected)
        {
            if (item != null)
            {
                DirtyFlag = true;
                item.Selected = bSelected;

                UpdateSelectedFolders(ProjectData.CurrentBackupProfile, item);
            }
        }

        bool m_bRefreshOnExpand = true;
        public void ExpandFolder(ItemCollection itemList)
        {
            foreach (var item in itemList)
            {
                var folderItem = item as FolderMenuItem;

                if (m_bRefreshOnExpand)
                {
                    folderItem.SourceBackupItems.Clear();
                }

                if (folderItem.SourceBackupItems.Count() == 0)
                {
                    AddFoldersToTree(folderItem);
                }
            }
        }


        void UpdateSelectedFolders(BackupProfileData profile, FolderMenuItem item)
        {
            SelectItemDown(item);
            SelectItemUp(item);

//            UpdateSelectedFolderList(profile);
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
                    //bool? bValue = item.Selected;
                    //foreach (var folder in parent.Items.Where(i => (i.IsFolder)))
                    //{
                    //    if (folder.Selected != bValue)
                    //    {
                    //        bValue = null;
                    //        break;
                    //    }
                    //}

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


        public void UpdateSelectedFolderList(BackupProfileData profile = null)
        {
            if (profile == null)
            {
                profile = ProjectData.CurrentBackupProfile;
            }

            //Update source folder selection list
            profile.FolderList.Clear();
            UpdateSelectedFolderListStep(profile, profile.RootFolderItemList);

            //update folder properties/size in UI window
            profile.UpdateProfileProperties();
        }
        void UpdateSelectedFolderListStep(BackupProfileData profile, ObservableCollection<FolderMenuItem> folderList)
        {
            foreach (var folder in folderList.Where(i => (i.IsFolder)))
            {
                if (folder.Selected == true)
                {
                    profile.FolderList.Add(new FolderData { Path = folder.Path });
                }
                else if (folder.Selected == null)
                {
                    UpdateSelectedFolderListStep(profile, folder.SourceBackupItems);
                }
            }
        }

    }
}
