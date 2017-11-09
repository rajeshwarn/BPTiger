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
    public class ProfileDataViewModel
    {
        public BackupProjectData Project { get;} = BackupProjectRepository.Instance.SelectedBackupProject;
        public BackupProfileData Profile { get;} = BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile;
    }
}
