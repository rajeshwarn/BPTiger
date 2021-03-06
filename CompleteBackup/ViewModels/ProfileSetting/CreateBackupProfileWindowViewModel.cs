﻿using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using CompleteBackup.ViewModels.FolderSelection.ICommands;
using CompleteBackup.ViewModels.ICommands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompleteBackup.ViewModels
{
    class CreateBackupProfileWindowViewModel
    {
        public ICommand CloseWindowCommand { get; private set; } = new CloseWindowICommand<object>();
        public ICommand CreateNewProfileCommand { get; private set; } = new CreateNewProfileICommand<object>();


        public BackupProjectData Project { get; } = BackupProjectRepository.Instance.SelectedBackupProject;

        public BackupProfileData Profile { get; } = new BackupProfileData()
        {
            BackupType = BackupTypeEnum.Snapshot,
            Name = "My Profile1",
            Description = "",
            BackupFolderList = new ObservableCollection<FolderData>()
            {
                //empty folder list
            },
        };

        public List<BackupTypeData> BackupTypeList { get; set; } = ProfileHelper.BackupTypeList;
    }
}
