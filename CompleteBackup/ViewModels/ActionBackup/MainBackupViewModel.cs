using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.ViewModels
{
    class MainBackupViewModel
    {
        public BackupProfileData Profile { get; set; } = BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile;

        public BackupItemsSelectionViewModel BackupItemsSelectionViewModel { get; set; } = new BackupItemsSelectionViewModel();
    }
}
