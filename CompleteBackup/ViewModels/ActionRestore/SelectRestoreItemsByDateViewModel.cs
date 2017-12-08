using CompleteBackup.DataRepository;
using CompleteBackup.Models.backup;
using CompleteBackup.Models.Backup.History;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using CompleteBackup.ViewModels.FolderSelection.ICommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompleteBackup.ViewModels
{
    public class SelectRestoreItemsByDateViewModel : ObservableObject
    {
        public SelectRestoreItemsByDateViewModel()
        {
            var profile = BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile;
            List<string> setList = BackupBase.GetBackupSetList_(profile);
            var firstSet = setList.FirstOrDefault();
            var firstSessionHistory = BackupSessionHistory.LoadHistory(profile.GetTargetBackupFolder(), firstSet);
            m_DisplayDateStart = firstSessionHistory.HistoryData.TimeStamp;

            var lastSet = setList.LastOrDefault();
            var lastSessionHistory = BackupSessionHistory.LoadHistory(profile.GetTargetBackupFolder(), lastSet);
            m_DisplayDateEnd = lastSessionHistory.HistoryData.TimeStamp;

            m_SelectedDate = m_DisplayDateEnd;
        }

        public ICommand CloseWindowCommand { get; private set; } = new CloseWindowICommand<object>();

        public BackupProjectData ProjectData { get; set; } = BackupProjectRepository.Instance.SelectedBackupProject;


        private DateTime m_SelectedDate;
        public DateTime SelectedDate { get { return m_SelectedDate; } set { m_SelectedDate = value; OnPropertyChanged(); } }

        private DateTime m_DisplayDateStart;
        public DateTime DisplayDateStart { get { return m_DisplayDateStart; } }

        private DateTime m_DisplayDateEnd;
        public DateTime DisplayDateEnd { get { return m_DisplayDateEnd; } }
    }
}
