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
    class ChangeBackupItemsWindowModel : BackupItemsTreeBase
    {
        public ICommand SaveFolderSelectionCommand { get; private set; } = new SaveFolderSelectionICommand<object>();

        public ChangeBackupItemsWindowModel() : base()
        {
            foreach (var item in ProjectData?.CurrentBackupProfile.FolderList)
            {
                SelectedItemList.Add(item);
            }
        }

        protected override FolderMenuItem CreateMenuItem(bool isFolder, bool isSelected, string path, string relativePath, string name, FolderMenuItem parentItem, FileAttributes attr)
        {
            var menuItem = new BackupFolderMenuItem()
            {
                IsFolder = isFolder,
                Attributes = attr,
                Path = path,
                RelativePath = relativePath,
                Name = name,
                ParentItem = parentItem,
                Selected = isSelected,
            };

            return menuItem;
        }


        protected override void AddRootItemsToTree()
        {
            var ProfileData = ProjectData.CurrentBackupProfile;

            ClearItemList();


            DriveInfo[] drives = DriveInfo.GetDrives();

            //Add all available drives to list
            foreach (var drive in drives)
            {
                DirectoryInfo dInfo = drive.RootDirectory;
                try
                {
                    FileAttributes attr = File.GetAttributes(drive.Name);
                    var rootItem = new BackupFolderMenuItem()
                    {
                        IsFolder = true,
                        Attributes = attr,
                        Path = drive.Name,
                        Name = $"{drive.VolumeLabel} ({drive.DriveType}) ({drive.Name})"
                    };

                    UpdateChildItemsInMenuItem(rootItem);

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        FolderMenuItemTree.Add(rootItem);
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
                    var match = FolderMenuItemTree.Where(f => String.Compare(f.Path, pr, true) == 0);

                    if (match.Count() > 0)
                    {
                        var item = AddPathToMenuItemTree(match.ElementAt(0), folder.Path);
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
    
        //Add full path (one selected folder/file) with all items to tree
        private FolderMenuItem AddPathToMenuItemTree(FolderMenuItem item, string path)
        {
            string pr = Directory.GetDirectoryRoot(path);
            var parentPath = Directory.GetParent(path);

            if (parentPath != null)
            {
                string name = parentPath.FullName;
                var parent = AddPathToMenuItemTree(item, name);

                var found = parent.SourceBackupItems.Where(i => String.Compare(i.Path, path, true) == 0);
                if (found.Count() == 0)
                {
                    FileAttributes attr = File.GetAttributes(path);
                    var newItem = new BackupFolderMenuItem() { IsFolder = true, Attributes = attr, Path = path, Name = path, ParentItem = parent, Selected = false };
                    parent.SourceBackupItems.Add(newItem);
                    return newItem;
                }
                else
                {
                    UpdateChildItemsInMenuItem(found.ElementAt(0));
                    return found.ElementAt(0);
                }
            }
            else
            {
                return item;
            }
        }

        protected override List<string> GetAllActiveSets(FolderMenuItem item)
        {
            return new List<string>() { item.Path };
        }

    }
}
