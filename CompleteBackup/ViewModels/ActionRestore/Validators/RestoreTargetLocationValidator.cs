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
    public class RestoreTargetLocationValidator : ValidationRule
    {

        public RestoreTargetLocationValidator() : base() { ValidatesOnTargetUpdated = true; }

        public override ValidationResult Validate (object value, System.Globalization.CultureInfo cultureInfo)
        {
            var name = value as String;
            var profile = BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile;

            if (profile == null)
            {
                return new ValidationResult(false, "Profile not fount or not selected");
            }
            else if ((name == null) || (name == String.Empty))
            {
                return new ValidationResult(false, "Please type or select a destenition restore folder");
            }
            else
            {
                var bExists = profile.GetStorageInterface().DirectoryExists(name);
                if (bExists)
                {
                    var attr = profile.GetStorageInterface().GetFileAttributes(name);
                    bool bRirectory = (attr & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory;
                    if (bRirectory)
                    {
                        return ValidationResult.ValidResult;
                    }
                    else
                    {
                        return new ValidationResult(false, "Destination folder is not a valid directory");
                    }
                }
                else
                {
                    return new ValidationResult(false, "Destination folder is not available or invalid");
                }
            }
        }
    }
}
