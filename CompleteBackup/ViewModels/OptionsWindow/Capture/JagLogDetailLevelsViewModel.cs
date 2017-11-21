using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using RadAnalytics.DataAccess;
using RadAnalytics.Utilities;
using RadAnalytics.JAG.Interfaces;
using RadAnalytics.Models;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Input;
using RadAnalytics.Models.FuelGenericService;

namespace RadAnalytics.ViewModels
{
    class JagLogDetailLevelsViewModel : ObservableObject, IPageViewModel
    {
        public JagLogDetailLevelsViewModel() { }

        public LoggingSettingData Settings
        {
            get { return PumpRepository.Instance.SelectedJagLogDetailLevelsData; }
        }

        public ObservableCollection<LoggingSettingData> JagNodes
        {
            get { return PumpRepository.Instance.JagLogDetailLevelsDataCollection; }
        }

        public int SelectedJagLogDetailLevelsIndex
        {
            get { return PumpRepository.Instance.SelectedJagLogDetailLevelsIndex; }
            set { PumpRepository.Instance.SelectedJagLogDetailLevelsIndex = value; }
        }        

        public string Name
        {
            get { return "Jag Log Details Levels Page"; }
        }
    }
}
