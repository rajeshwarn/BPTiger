using CompleteBackup.DataRepository;
using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CompleteBackup.ViewModels.FolderSelection.Validators
{
    public class BackupTargetLocationValidator : ValidationRule
    {

        public BackupTargetLocationValidator() : base() { ValidatesOnTargetUpdated = true; }

        public override ValidationResult Validate (object value, System.Globalization.CultureInfo cultureInfo)
        {
            var name = value as String;
            var profile = BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile;

            if (profile == null)
            {
                return new ValidationResult(false, "Please create or select a Backup Profile");
            }
            else
            if ((name == null) || (name == String.Empty))
            {
                return new ValidationResult(false, "Please Enter or select a destenition backup folder");
            }
            else
            {
                var folderStatus = profile.GetProfileTargetFolderStatus(name);
                if (folderStatus == Models.Backup.Profile.BackupProfileData.ProfileTargetFolderStatusEnum.AssosiatedWithThisProfile ||
                    folderStatus == Models.Backup.Profile.BackupProfileData.ProfileTargetFolderStatusEnum.EmptyFolderNoProfile)
                {
                    return ValidationResult.ValidResult;
                }
                else
                {
                    var num = (int)folderStatus;
                    var res = BackupProfileData.ProfileTargetFolderStatusDictionary[folderStatus];
                    return new ValidationResult(false, BackupProfileData.ProfileTargetFolderStatusDictionary[folderStatus]);
                }
            }
        }
    }
}
