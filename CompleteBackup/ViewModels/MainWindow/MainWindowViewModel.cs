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

        public ICommand RestoreBackupCommand { get; private set; } = new RestoreBackupICommand<object>();


        public static ObservableCollection<BackupTypeData> BackupTypeList { get; private set; } = new ObservableCollection<BackupTypeData>() {
            new BackupTypeData() { BackupType = BackupTypeEnum.Full, Name = "Full Backup", ImageName = @"/Resources/Icons/Ribbon/FullBackup.ico", Description = "A full copy of your entire data set"},
            new BackupTypeData() { BackupType = BackupTypeEnum.Incremental, Name = "Incremental Backup", ImageName = @"/Resources/Icons/Ribbon/IncrementalBackup.ico", Description = "Starts with a full backup, and subsequent backups only backup data that has changed\nFor a faster backup"},
            new BackupTypeData() { BackupType = BackupTypeEnum.Differential, Name = "Differential Backup", ImageName = @"/Resources/Icons/Ribbon/DifferentialBackup.ico", Description = "Similar to Incremental Backup, but also contains all the data that changed since the first backup\nThis lets you restore a specific version event if the data has been deleted in the source"},
        };

        private BackupTypeData m_BackupTypeData;
        public BackupTypeData ProfileBackupType
        {
            get
            {
                //return BackupTypeList.FirstOrDefault(i => i.BackupType == ProjectData.CurrentBackupProfile.BackupType);
                return m_BackupTypeData;
            }
            set
            {
                m_BackupTypeData = value;
                OnPropertyChanged();
            }
        }

        const int m_DefaultPageIndex = 2;

        public MainWindowViewModel()
        {
            ProfileBackupType = BackupTypeList.FirstOrDefault(i => i.BackupType == ProjectData.CurrentBackupProfile.BackupType);

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
