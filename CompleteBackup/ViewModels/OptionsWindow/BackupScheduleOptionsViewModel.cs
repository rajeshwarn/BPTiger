using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using System.Windows.Threading;
using System.Threading;
using System.Windows.Input;
using CompleteBackup;
using CompleteBackup.ViewModels;

namespace CompleteBackup.ViewModels
{
    class BackupScheduleOptionsViewModel : ObservableObject, IPageViewModel
    {
        public string Name { get { return "Backup Schedule Options"; } }

        public Properties.BackupSchedule Settings { get { return Properties.BackupSchedule.Default; } }
    }
}
