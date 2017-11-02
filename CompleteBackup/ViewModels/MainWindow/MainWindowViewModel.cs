using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
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
        public ICommand StartBackupCommand { get; private set; } = new StartBackupICommand<object>();
        public ICommand PauseBackupCommand { get; private set; } = new PauseBackupICommand<object>();
        public ICommand StopBackupCommand { get; private set; } = new StopBackupICommand<object>();

        public ICommand SelectBackupProfileCommand { get; private set; } = new SelectBackupProfileICommand<object>();
        

        public ICommand RestoreBackupCommand { get; private set; } = new RestoreBackupICommand<object>();

        const int m_DefaultPageIndex = 2;

        public MainWindowViewModel()
        {
            m_MainViewDictionary = new Dictionary<int, object>()
            {
                { 0, new MainProfileViewModel() },
                { 1, new MainBackupViewModel() },
                { 2, new MainRestoreViewModel() },
            };

            m_CurrentPageViewModel = m_MainViewDictionary[m_DefaultPageIndex];
        }

        private Dictionary<int, object> m_MainViewDictionary;


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
                CurrentPageViewModel = m_MainViewDictionary[m_SelectedRibbonIndex];
            }
            catch (KeyNotFoundException) { }
        }
    }
}
