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
    class RestoreBackupItemsWindowModel : BackupItemsTreeBase
    {
        public ICommand SaveFolderSelectionCommand { get; private set; } = new SaveFolderSelectionICommand<object>();


        public ObservableCollection<RestoreFolderMenuItem> RestoreItemList { get; set; } = new ObservableCollection<RestoreFolderMenuItem>();


        private bool m_DirtyFlag = false;
        public bool DirtyFlag { get { return m_DirtyFlag; } set { m_DirtyFlag = value; OnPropertyChanged(); } }

        protected override FolderMenuItem CreateMenuItem(bool isFolder, bool isSelected, string path, string name, FolderMenuItem parentItem, FileAttributes attr)
        {
            var menuItem = new RestoreFolderMenuItem()
            {
                IsFolder = isFolder,
                Path = path,
                Name = name,
                ParentItem = parentItem,
                Selected = isSelected
            };

            return menuItem;
        }

        protected override void AddRootItemsToTree()
        {
            var profile = ProjectData.CurrentBackupProfile;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                RestoreItemList.Clear();
            }));

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
                            InsertNamesToTree(sessionHistory, item, item.Path, 0);
                        }
                    }
                    else
                    {
                        foreach (var item in sessionHistory.HistoryItemList.Where(i => i.HistoryType != HistoryTypeEnum.NoChange))
                        {
                            InsertNamesToTree(sessionHistory, item, item.Path, 0);
                        }
                    }
                }
            }));
        }

        FolderMenuItem InsertNamesToTree(BackupSessionHistory history, HistoryItem item, string path, int iCount)
        {
            if (path != null)
            {
                var name = m_IStorage.GetFileName(path);
                if ((name != null) && (name != string.Empty))
                {

                    var newPath = path.Substring(0, path.Length - name.Length - 1);

                    var menuItem = InsertNamesToTree(history, item, newPath, iCount + 1);
                    var newMenuItem = menuItem.SourceBackupItems.Where(m => m.Name == name).FirstOrDefault();
                    if (newMenuItem == null)
                    {
                        newMenuItem = new RestoreFolderMenuItem() { IsFolder = true, Path = path, Name = name};
                        menuItem.SourceBackupItems.Add(newMenuItem);
                    }

                    if (iCount == 0)
                    {
                        var setTimeMenuItem = new RestoreFolderMenuItem() { Name = history.TimeStamp.ToString(), IsFolder = true, HistoryType = item.HistoryType };
                        newMenuItem.SourceBackupItems.Add(setTimeMenuItem);
                    }

                    return newMenuItem;
                }
                else
                {
                    name = path;

                    var menuItem = RestoreItemList.Where(m => m.Path == name).FirstOrDefault();
                    if (menuItem == null)
                    {
                        menuItem = new RestoreFolderMenuItem() { IsFolder = true, Path = path, Name = name};
                        RestoreItemList.Add(menuItem);
                    }

                    return menuItem;
                }
            }

            return null;
        }

      
    


        IStorageInterface m_IStorage = new FileSystemStorage();

        //FolderMenuItem AddFolderName(FolderMenuItem item, string path)
        //{
        //    string pr = Directory.GetDirectoryRoot(path);
        //    var parentPath = Directory.GetParent(path);

        //    if (parentPath != null)
        //    {
        //        string name = parentPath.FullName;
        //        var parent = AddFolderName(item, name);

        //        var found = parent.SourceBackupItems.Where(i => String.Compare(i.Path, path, true) == 0);
        //        if (found.Count() == 0)
        //        {
        //            FileAttributes attr = File.GetAttributes(path);
        //            var newItem = new FolderMenuItem() { IsFolder = true, Attributes = attr, Path = path, Name = path, ParentItem = parent, Selected = false };
        //            parent.SourceBackupItems.Add(newItem);
        //            return newItem;
        //        }
        //        else
        //        {
        //            AddFoldersToTree(found.ElementAt(0));
        //            return found.ElementAt(0);
        //        }

        //    }
        //    else
        //    {
        //        return item;
        //    }
        //}

        //void AddFoldersToTree(FolderMenuItem item)
        //{
        //    if (item.IsFolder)
        //    {
        //        var sourceSubdirectoryEntriesList = GetDirectoriesNames(item.Path);
        //        foreach (string subdirectory in sourceSubdirectoryEntriesList)
        //        {
        //            string newPath = m_IStorage.Combine(item.Path, subdirectory);


        //            //MOVE THIS TO FS class
        //            FileAttributes attr = 0;
        //            if (newPath.Length < Win32LongPathFile.MAX_PATH)
        //            {
        //                attr = File.GetAttributes(newPath);
        //            }
        //            else
        //            {
        //                attr = (FileAttributes)Win32FileSystem.GetFileAttributesW(Win32LongPathFile.GetWin32LongPath(newPath));
        //            }

        //            if (((attr & FileAttributes.System) != FileAttributes.System) &&
        //                ((attr & FileAttributes.Hidden) != FileAttributes.Hidden))
        //            {
        //                var newItem = new FolderMenuItem() { IsFolder = true, Attributes = attr, Path = newPath, Name = subdirectory, ParentItem = item, Selected = false };
        //                item.SourceBackupItems.Add(newItem);

        //                //var match = ProjectData.FolderList.Where(f => String.Compare(f, newPath, true) == 0);

        //                //var iCount = match.Count();
        //                //if (iCount > 0)
        //                //{
        //                //    newItem.Selected = true;
        //                //}
        //                //else
        //                //{
        //                if (item.Selected == true)
        //                {
        //                    newItem.Selected = true;
        //                }
        //                else
        //                {
        //                    newItem.Selected = false;
        //                }
        //                //     }

        //                //SelectItemUp(newItem);

        //                //else if ((match != null) && (match.Count() > 0))
        //                //{
        //                //    SelectItemUp(newItem, true);
        //                //}
        //            }
        //        }

        //        var fileList = m_IStorage.GetFiles(item.Path);
        //        foreach (var file in fileList)
        //        {
        //            var filePath = m_IStorage.Combine(item.Path, file);
        //            FileAttributes attr = File.GetAttributes(filePath);

        //            item.SourceBackupItems.Add(new FolderMenuItem() { IsFolder = false, Attributes = attr, Name = file, Path = filePath, Image = null });
        //        }
        //    }
        //}

        //protected List<string> GetDirectoriesNames(string path)
        //{
        //    //Process directories
        //    string[] sourceSubdirectoryEntries = m_IStorage.GetDirectories(path);
        //    var sourceSubdirectoryEntriesList = new List<string>();
        //    if (sourceSubdirectoryEntries != null)
        //    {
        //        if (sourceSubdirectoryEntriesList != null)
        //        {
        //            foreach (var entry in sourceSubdirectoryEntries)
        //            {
        //                sourceSubdirectoryEntriesList.Add(m_IStorage.GetFileName(entry));
        //            }
        //        }
        //    }

        //    return sourceSubdirectoryEntriesList;
        //}


        public void FolderTreeClick(RestoreFolderMenuItem item, bool bSelected)
        {
            if (item != null)
            {
                DirtyFlag = true;
                item.Selected = bSelected;

          //      UpdateSelectedFolders(ProjectData.CurrentBackupProfile, item);
            }
        }

        bool m_bRefreshOnExpand = true;
        public void ExpandFolder(ItemCollection itemList)
        {
            return;

            foreach (var item in itemList)
            {
                var folderItem = item as RestoreFolderMenuItem;

                if (m_bRefreshOnExpand)
                {
                    folderItem.SourceBackupItems.Clear();
                }

                if (folderItem.SourceBackupItems.Count() == 0)
                {
                    //AddFoldersToTree(folderItem);
                }
            }
        }



    }
}
