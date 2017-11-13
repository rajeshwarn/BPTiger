using CompleteBackup.DataRepository;
using CompleteBackup.ViewModels.ICommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.Profile
{
    class ProfileListViewModel
    {
        public ICommand StartFullBackupCommand { get; private set; } = new StartFullBackupICommand<object>();
        public ICommand PauseBackupCommand { get; private set; } = new PauseBackupICommand<object>();
        public ICommand StopBackupCommand { get; private set; } = new StopBackupICommand<object>();


        public ICommand OpenCreateProfileWindowCommand { get; } = new OpenCreateProfileWindowICommand<object>();
        public ICommand OpenEditProfileWindowCommand { get; } = new OpenEditProfileWindowICommand<object>();

        public ICommand DeleteBackupProfileCommand { get; } = new DeleteBackupProfileICommand<object>();


        public ICommand CancelPendingBackupTaskCommand { get; } = new CancelPendingBackupTaskICommand<object>();
        

        public BackupProjectRepository Repository { get; } = BackupProjectRepository.Instance;
    }
}
