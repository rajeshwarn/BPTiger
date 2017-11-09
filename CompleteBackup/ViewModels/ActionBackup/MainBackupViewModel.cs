using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.ViewModels
{
    class MainBackupViewModel
    {
        public BackupProjectData Project { get; set; } = BackupProjectRepository.Instance.SelectedBackupProject;

        public BackupItemsSelectionViewModel BackupItemsSelectionViewModel { get; set; } = new BackupItemsSelectionViewModel();
    }
}
