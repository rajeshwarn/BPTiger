using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using CompleteBackup.ViewModels.FolderSelection.ICommands;
using CompleteBackup.ViewModels.ICommands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.MainWindow
{
    class MainWindowViewModel : ObservableObject
    {
        public ICommand OpenSelectBackupItemsWindowCommand { get; private set; } = new OpenSelectBackupItemsWindowICommand<object>();
        public ICommand SelectTargetBackupFolderNameCommand { get; private set; } = new SelectTargetBackupFolderNameICommand<object>();
        public ICommand SelectTargetRestoreFolderNameCommand { get; private set; } = new SelectTargetRestoreFolderNameICommand<object>();

        public ICommand OpenSelectRestoreItemsWindowCommand { get; private set; } = new OpenSelectRestoreItemsWindowICommand<object>();
        

        public ICommand ClearLogConsoleCommand { get; private set; } = new ClearLogConsoleICommand<object>();
        public ICommand StartWatcherBackupCommand { get; private set; } = new StartWatcherBackupICommand<object>();
        public ICommand StartFullBackupCommand { get; private set; } = new StartFullBackupICommand<object>();
        public ICommand PauseBackupCommand { get; private set; } = new PauseBackupICommand<object>();
        public ICommand StopBackupCommand { get; private set; } = new StopBackupICommand<object>();

        public ICommand SelectBackupProfileCommand { get; private set; } = new SelectBackupProfileICommand<object>();
        

        public ICommand RestoreBackupCommand { get; private set; } = new RestoreBackupICommand<object>();

        const int m_DefaultPageIndex = 1;

        public MainWindowViewModel()
        {
            m_MainViewArray = new object[]
            {
                new MainProfileViewModel(),
                new MainBackupViewModel(),
                new MainRestoreViewModel(),
                new LogConsoleViewModel(),
            };

            m_CurrentPageViewModel = m_MainViewArray[m_DefaultPageIndex];
        }

        private object[] m_MainViewArray;


        private object m_CurrentPageViewModel;
        public object CurrentPageViewModel { get { return m_CurrentPageViewModel; } set { m_CurrentPageViewModel = value; OnPropertyChanged(); } }

        private int m_SelectedRibbonIndex = m_DefaultPageIndex;
        public int SelectedRibbonIndex { get { return m_SelectedRibbonIndex; } set { m_SelectedRibbonIndex = value; OnPropertyChanged(); UpdateCurrentMainView(); } }



        public BackupProjectRepository Project { get; set; } = BackupProjectRepository.Instance;
        public BackupProjectData ProjectData { get; set; } = BackupProjectRepository.Instance.SelectedBackupProject;




        public void UpdateCurrentMainView()
        {
            try
            {
                CurrentPageViewModel = m_MainViewArray[m_SelectedRibbonIndex];
                if (CurrentPageViewModel is MainProfileViewModel)
                {
                    var profileView = CurrentPageViewModel as MainProfileViewModel;
                    profileView.ProfileData.UpdateProfileProperties();
                }

                //ProjectData.CurrentBackupProfile?.UpdateProfileProperties();
            }
            catch (Exception)
            {

            }
        }
    }
}
