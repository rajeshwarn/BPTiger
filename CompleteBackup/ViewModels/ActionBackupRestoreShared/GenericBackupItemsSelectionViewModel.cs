﻿using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Models.FolderSelection;
using CompleteBackup.ViewModels.FolderSelection.ICommands;
using CompleteBackup.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace CompleteBackup.ViewModels
{
    public abstract class GenericBackupItemsSelectionViewModel : ObservableObject
    {
        public GenericBackupItemsSelectionViewModel()
        {
            m_CurrentBackupProfile = BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile;

            //Register to get update event when backup profile changed
            BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile?.ProfileDataRefreshTask?.RegisterEvent(ProfileDataUpdate);
        }
        private void ProfileDataUpdate(BackupProfileData profile)
        {
        }
                
        public abstract ICommand OpenItemSelectWindowCommand { get; }
        public ICommand SelectFolderNameCommand { get; private set; } = new SelectFolderNameICommand<object>();

        public ICommand LocateMissingItemCommand { get; private set; } = new LocateMissingItemICommand<object>();

        public ICommand OpenItemLocationCommand { get; private set; } = new OpenItemLocationICommand<object>();

        public BackupProjectData ProjectData { get; set; } = BackupProjectRepository.Instance.SelectedBackupProject;

        public void UpdateCurrentProfileChange()
        {
            OnPropertyChanged("DestinationFolderName");
            OnPropertyChanged("SelectionFolderList");
            OnPropertyChanged("SelectionFolderListNumberOfFiles");
            OnPropertyChanged("SelectionTotalFolderListSize");
        }

        public BackupProfileData m_CurrentBackupProfile;
        public BackupProfileData CurrentBackupProfile { get { return m_CurrentBackupProfile; } set { m_CurrentBackupProfile = value; OnPropertyChanged(); } }

        public abstract ObservableCollection<FolderData> SelectionFolderList { get; }
        public abstract string DestinationFolderName { get; set; }

        public abstract long SelectionFolderListNumberOfFiles { get; }
        public abstract long SelectionTotalFolderListSize { get; }


        public abstract string SourceFileListGroupTitle { get; }
        public abstract string SourceFileActionTitle { get; }
        public abstract string DestinationGroupTitle { get; }
    }
}

