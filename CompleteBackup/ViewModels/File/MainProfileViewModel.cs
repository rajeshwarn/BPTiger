using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Views.ExtendedControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CompleteBackup.ViewModels
{
    class MainProfileViewModel : ObservableObject
    {
        public MainProfileViewModel()
        {
            ProfileGaugeList.Add(new ChartGaugeView(Brushes.Red, Brushes.Green, Brushes.Yellow) { PumpNumber = 0, GaugeValue = 0.6F });
           // ProfileGaugeList.Add(new ChartGaugeView(Brushes.Red, Brushes.Green, Brushes.Yellow) { PumpNumber = 1, GaugeValue = 0.2F });

            //Register to get update event when backup profile changed
            BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile.RegisterEvent(ProfileDataUpdate);

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
