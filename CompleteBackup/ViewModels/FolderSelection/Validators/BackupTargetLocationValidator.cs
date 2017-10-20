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
            var backupSet = BackupProjectRepository.Instance.SelectedBackupSet;


            if ((name == null) || (name == String.Empty))
            {
                return new ValidationResult(false, "Please Enter or select a destenition backup folder");
            }
            else
            {
                if (backupSet.IsValidSetData)
                {
                    return ValidationResult.ValidResult;
                }
                else
                {
                    return new ValidationResult (false, "The path does not point to a valid folder");
                }
            }
        }
    }
}
