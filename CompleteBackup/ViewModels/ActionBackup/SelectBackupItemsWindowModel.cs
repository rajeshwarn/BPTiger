﻿using CompleteBackup.DataRepository;
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
    class SelectBackupItemsWindowModel : BackupItemsTreeBase
    {
        public ICommand SaveFolderSelectionCommand { get; private set; } = new SaveFolderSelectionICommand<object>();

        public SelectBackupItemsWindowModel() : base()
        {
            foreach (var item in ProjectData?.CurrentBackupProfile.BackupFolderList.Where(i => i.IsAvailable))
            {
                SelectedItemList.Add(item);
            }
        }

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
            var ProfileData = ProjectData.CurrentBackupProfile;

            ClearItemList();


            DriveInfo[] drives = DriveInfo.GetDrives();

            //Add all available drives to list
            foreach (var drive in drives)
            {
                DirectoryInfo dInfo = drive.RootDirectory;

                var name = drive.Name;
                FileAttributes attr = 0;
                try
                {
                    attr = File.GetAttributes(drive.Name);
                    name = $"{drive.VolumeLabel} ({drive.DriveType}) ({drive.Name})";
                }
                catch(IOException)
                {
                    name = name + " [Not Ready]";
                }

                var rootItem = new BackupFolderMenuItem()
                {
                    IsFolder = true,
                    Attributes = attr,
                    Path = drive.Name,
                    RelativePath = drive.Name,
                    Name = name,
                };

                try
                {
                    UpdateChildItemsInMenuItem(rootItem);
                }
                catch (Exception) { }

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    FolderMenuItemTree.Add(rootItem);
                }));
            }

            //Add selected folders to tree list
            var itemList = new List<FolderMenuItem>();
    //        Application.Current.Dispatcher.Invoke(new Action(() =>
    //        {
                foreach (var folder in SelectedItemList)
                {
                    string pr = Directory.GetDirectoryRoot(folder.Path);
                    var match = FolderMenuItemTree.FirstOrDefault(f => String.Compare(f.Path, pr, true) == 0);

                    if (match != null)
                    {
                        try
                        {
                            var item = AddPathToMenuItemTree(match, folder.Path);
                            itemList.Add(item);
                        }
                        catch (FileNotFoundException)
                        {
                            ProfileData.Logger.Writeln($"***Warning: Backup item not available: {folder.Path}");
                        }
                    }
                }
 //           }));

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

                var found = parent.ChildFolderMenuItems.FirstOrDefault(i => String.Compare(i.Path, path, true) == 0);
                if (found == null)
                {
                    FileAttributes attr = File.GetAttributes(path);
                    var newItem = new BackupFolderMenuItem() { IsFolder = true, Attributes = attr, Path = path, Name = path, ParentItem = parent, Selected = false };
                    parent.ChildFolderMenuItems.Add(newItem);
                    return newItem;
                }
                else
                {
                    UpdateChildItemsInMenuItem(found);
                    return found;
                }
            }
            else
            {
                return item;
            }
        }


        protected override void AddFilesToFolderMenuItem(FolderMenuItem item, string itemPath, BackupSessionHistory history)
        {
            //            AddFilesToFolderMenuItemBaseXXX(item, itemPath, history);
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
    }
}
