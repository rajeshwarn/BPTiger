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

        public List<BackupTypeData> BackupTypeList { get; set; } = ProfileHelper.BackupTypeList;


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
                if (m_BackupTypeData != value)
                {
                    if (m_BackupTypeData != null)
                    {
                        var result = MessageBox.Show($"Are you sure you want to change the profile backup type?", "Backup Type", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                        if (result != MessageBoxResult.Yes)
                        {
                            return;
                        }
                    }

                    ProjectData.CurrentBackupProfile.SetBackupType(value.BackupType);

                    m_BackupTypeData = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainProfileViewModel()
        {
            ProfileBackupType = ProfileHelper.BackupTypeList.FirstOrDefault(i => i.BackupType == ProjectData.CurrentBackupProfile.BackupType);

            ProfileGaugeList.Add(new ChartGaugeView(Brushes.Red, Brushes.Green, Brushes.Yellow) { PumpNumber = 0, GaugeValue = 0.6F });
            // ProfileGaugeList.Add(new ChartGaugeView(Brushes.Red, Brushes.Green, Brushes.Yellow) { PumpNumber = 1, GaugeValue = 0.2F });

            //Register to get update event when backup profile changed
            ProjectData.CurrentBackupProfile.RegisterEvent(ProfileDataUpdate);

        }

        ~MainProfileViewModel()
        {
            BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile.UnRegisterEvent(ProfileDataUpdate);
        }

        public BackupProjectRepository Repository { get; } = BackupProjectRepository.Instance;
        public ObservableCollection<ChartGaugeView> ProfileGaugeList { get; set; } = new ObservableCollection<ChartGaugeView>();


        private void ProfileDataUpdate(BackupProfileData profile)
        {
            float ratio = (float)profile.m_BackupTargetUsedSizeNumber / (float)profile.m_BackupTargetDiskSizeNumber;

            ProfileGaugeList[0].GaugeValue = ratio;
        }
    }
}
