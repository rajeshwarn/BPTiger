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
    class BackupGeneralOptionsViewModel : ObservableObject, IPageViewModel
    {
        public string Name { get { return "Database General"; } }

        public Properties.General Settings { get { return Properties.General.Default; } }
    }
}
