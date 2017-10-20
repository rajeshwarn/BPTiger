using CompleteBackup.DataRepository;
using CompleteBackup.Models.backup;
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
        public override ValidationResult Validate (object value, System.Globalization.CultureInfo cultureInfo)
        {
            var name = value as String;
            var backupProfile = BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile;

            if (backupProfile == null)
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
                var folderStatus = backupProfile.GetProfileTargetFolderStatus(name);
                if (folderStatus == Models.Backup.Profile.BackupProfileData.ProfileTargetFolderStatusEnum.AssosiatedWithThisProfile)
                {
                    return ValidationResult.ValidResult;
                }
                else
                {
                    return new ValidationResult(false, folderStatus.ToString());
                }
            }
        }
    }
}
