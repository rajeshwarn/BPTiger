using CompleteBackup.DataRepository;
using CompleteBackup.ViewModels.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.ViewModels.MainWindow
{
    class MainWindowViewModel
    {
        public MainWindowViewModel()
        {

        }

        public object CurrentPageViewModel { get; set; } = new MainBackupViewModel();// ProfileListViewModel();

        public BackupProjectRepository Project { get; set; } = BackupProjectRepository.Instance;
        public string TEXT { get; set; } = "guy test";
    }
}
