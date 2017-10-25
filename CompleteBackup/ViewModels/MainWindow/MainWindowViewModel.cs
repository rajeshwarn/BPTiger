using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Project;
using CompleteBackup.ViewModels.MainWindow.ICommands;
using CompleteBackup.ViewModels.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.ViewModels.MainWindow
{
    class MainWindowViewModel : ObservableObject
    {
        public System.Windows.Input.ICommand StartBackupCommand { get; private set; } = new StartBackupICommand<object>();

        public MainWindowViewModel()
        {

            m_MainViewDictionary = new Dictionary<int, object>()
            {
                { 1, new MainBackupViewModel() },
                { 2, new MainRestoreViewModel() },
            };

            m_CurrentPageViewModel = m_MainViewDictionary[1];
        }

        private Dictionary<int, object> m_MainViewDictionary;


        private object m_CurrentPageViewModel;
        public object CurrentPageViewModel { get { return m_CurrentPageViewModel; } set { m_CurrentPageViewModel = value; OnPropertyChanged(); } }

        private int m_SelectedRibbonIndex = 1;
        public int SelectedRibbonIndex { get { return m_SelectedRibbonIndex; } set { m_SelectedRibbonIndex = value; OnPropertyChanged(); UpdateCurrentMainView(); } }
        

        public BackupProjectRepository Project { get; set; } = BackupProjectRepository.Instance;
        public BackupProjectData ProjectData { get; set; } = BackupProjectRepository.Instance.SelectedBackupProject;




        private void UpdateCurrentMainView()
        {
            try
            {
                CurrentPageViewModel = m_MainViewDictionary[m_SelectedRibbonIndex];
            }
            catch (KeyNotFoundException) { }
        }
    }
}
