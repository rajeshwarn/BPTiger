using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.ViewModels.ICommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompleteBackup.ViewModels
{
    class SelectBackupTypeWindowViewModel
    {
        public ICommand SelectBackupTypeCommand { get; private set; } = new SelectBackupTypeICommand<object>();
        public BackupProjectRepository Project { get; set; } = BackupProjectRepository.Instance;
        public List<BackupTypeData> BackupTypeList { get; set; } = ProfileHelper.BackupTypeList;


        public SelectBackupTypeWindowViewModel()
        {
            var Profile = Project.SelectedBackupProject.CurrentBackupProfile;
            foreach (var item in BackupTypeList)
            {
                if (item.BackupType == Profile.BackupType)
                {
                    item.IsChecked = true;
                }
                else
                {
                    item.IsChecked = false;
                }
            }
        }
    }
}
