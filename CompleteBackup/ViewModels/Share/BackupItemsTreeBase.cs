using CompleteBackup.DataRepository;
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
using System.Windows.Input;

namespace CompleteBackup.ViewModels
{
    abstract class BackupItemsTreeBase : ObservableObject
    {
        public ICommand CloseWindowCommand { get; private set; } = new CloseWindowICommand<object>();

        public BackupProjectData ProjectData { get; set; } = BackupProjectRepository.Instance.SelectedBackupProject;

        public ObservableCollection<FolderMenuItem> m_MenuItemTree;

        protected IStorageInterface m_IStorage;

        public BackupItemsTreeBase()
        {
            m_IStorage = ProjectData.CurrentBackupProfile.GetStorageInterface();
            new Thread(new System.Threading.ThreadStart(() =>
            {
                AddRootItemsToTree();
            })) { IsBackground = true, Name = "Folder Tree Selection" }.Start();
        }

        protected virtual void SetMenuItemTree(ObservableCollection<FolderMenuItem> menuItemTree)
        {
            m_MenuItemTree = menuItemTree;
        }

        protected virtual void ClearItemList()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                m_MenuItemTree?.Clear();
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
                    var filePath = m_IStorage.Combine(item.Path, file);
                    FileAttributes attr = File.GetAttributes(filePath);
                    if (!IsHidden(attr))
                    {
                        bool bSelected = false;
                        item.SourceBackupItems.Add(CreateMenuItem(false, bSelected, filePath, file, item, attr));
                    }
                }
            }
        }

    }
}
