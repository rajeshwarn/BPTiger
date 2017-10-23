using CompleteBackup.DataRepository;
using CompleteBackup.Views.ExtendedControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CompleteBackup.ViewModels.Profile
{
    class ProfileDetailsPageViewModel : ObservableObject
    {
        public ProfileDetailsPageViewModel()
        {
//            ProfileGaugeList.Add(new ChartGaugeView(Brushes.Red, Brushes.Green, Brushes.Yellow) { PumpNumber = 0, GaugeValue = 66 });
//            ProfileGaugeList.Add(new ChartGaugeView(Brushes.Red, Brushes.Green, Brushes.Yellow) { PumpNumber = 1, GaugeValue = 66 });
        }

        public BackupProjectRepository Repository { get; } = BackupProjectRepository.Instance;
        public ObservableCollection<ChartGaugeView> ProfileGaugeList { get; set; } = new ObservableCollection<ChartGaugeView>();
    }
}
