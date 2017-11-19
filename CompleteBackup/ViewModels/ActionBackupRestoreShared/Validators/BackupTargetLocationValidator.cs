using CompleteBackup.DataRepository;
using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CompleteBackup.ViewModels.FolderSelection.Validators
{
    public class GenericBackupItemsSelectionViewModelContext : DependencyObject
    {

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(@"ViewModel",
            typeof(GenericBackupItemsSelectionViewModel), typeof(GenericBackupItemsSelectionViewModelContext),
            new PropertyMetadata
            {
                DefaultValue = null,
                PropertyChangedCallback = new PropertyChangedCallback(GenericBackupItemsSelectionViewModelContext.ViewModelPropertyChanged)
            });

        public GenericBackupItemsSelectionViewModel ViewModel
        {
            get { return (GenericBackupItemsSelectionViewModel)this.GetValue(GenericBackupItemsSelectionViewModelContext.ViewModelProperty); }
            set { this.SetValue(GenericBackupItemsSelectionViewModelContext.ViewModelProperty, value); }
        }

        private static void ViewModelPropertyChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
        }

    }

    public class BackupTargetLocationValidator : ValidationRule
    {

        public BackupTargetLocationValidator() : base() { ValidatesOnTargetUpdated = true; }

        public GenericBackupItemsSelectionViewModelContext Context { get; set; }

        public override ValidationResult Validate (object value, System.Globalization.CultureInfo cultureInfo)
        {
            var name = value as String;
            var profile = BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile;

            if (profile == null)
            {
                return new ValidationResult(false, "Please create or select a Backup Profile");
            }
            else if ((name == null) || (name == String.Empty))
            {
                return new ValidationResult(false, "Please Enter or select a destenition backup folder");
            }
            else if (Context.ViewModel is BackupItemsSelectionViewModel)
            {
                var folderStatus = profile.GetProfileTargetFolderStatus(name);
                if (folderStatus == ProfileTargetFolderStatusEnum.AssosiatedWithThisProfile ||
                    folderStatus == ProfileTargetFolderStatusEnum.EmptyFolderNoProfile)
                {
                    return ValidationResult.ValidResult;
                }
                else
                {
                    var num = (int)folderStatus;
                    var res = ProfileHelper.ProfileTargetFolderStatusDictionary[folderStatus];
                    return new ValidationResult(false, ProfileHelper.ProfileTargetFolderStatusDictionary[folderStatus]);
                }
            }
            else
            {
                if (profile.GetStorageInterface().DirectoryExists(name) && profile.GetStorageInterface().IsFolder(name))
                {
                    return ValidationResult.ValidResult;
                }
                else
                {
                    return new ValidationResult(false, "Restore directory not valid or not available");
                }
            }
        }
    }
}
