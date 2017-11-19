using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using CompleteBackup.Views.ExtendedControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CompleteBackup.ViewModels
{
    class MainProfileViewModel : ObservableObject
    {
        public BackupProjectData ProjectData { get; set; } = BackupProjectRepository.Instance.SelectedBackupProject;
        public BackupProfileData ProfileData { get; set; } = BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile;

//        public List<BackupRunTypeEnum> BackupRunTypeList { get; set; } = new List<BackupRunTypeEnum> { BackupRunTypeEnum.Always, BackupRunTypeEnum.Manual };

        public List<BackupTypeData> BackupTypeList { get; set; } = ProfileHelper.BackupTypeList;


        private BackupTypeData m_BackupTypeData;
        public BackupTypeData ProfileBackupType { get { return m_BackupTypeData; } set { m_BackupTypeData = value; OnPropertyChanged(); } }

        public MainProfileViewModel()
        {
            ProfileBackupType = ProfileHelper.BackupTypeList.FirstOrDefault(i => i.BackupType == ProjectData.CurrentBackupProfile?.BackupType);

            ProfileGaugeList.Add(new ChartGaugeView(Brushes.Red, Brushes.Green, Brushes.Yellow) { PumpNumber = null, GaugeValue = 0.6F });

            //Register to get update event when backup profile changed
            ProjectData.CurrentBackupProfile?.ProfileDataRefreshTask?.RegisterEvent(ProfileDataUpdateEvent);
        }

        ~MainProfileViewModel()
        {
            ProjectData.CurrentBackupProfile?.ProfileDataRefreshTask?.UnRegisterEvent(ProfileDataUpdateEvent);
            //   BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile.ProfileDataRefreshTask?.UnRegisterEvent(ProfileDataUpdate);
        }

        public BackupProjectRepository Repository { get; } = BackupProjectRepository.Instance;
        public ObservableCollection<ChartGaugeView> ProfileGaugeList { get; set; } = new ObservableCollection<ChartGaugeView>();


        private void ProfileDataUpdateEvent(BackupProfileData profile)
        {
            float ratio = (float)profile.BackupTargetUsedSizeNumber / (float)profile.BackupTargetDiskSizeNumber;

            ProfileGaugeList[0].GaugeValue = ratio;
        }
    }
}
