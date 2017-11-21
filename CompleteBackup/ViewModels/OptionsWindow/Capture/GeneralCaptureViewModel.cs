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

namespace RadAnalytics.ViewModels
{
    class GeneralCaptureViewModel : ObservableObject, IPageViewModel
    {
        public GeneralCaptureViewModel() { }

        public ICommand SelectFolderNameCommand { get; private set; } = new SelectFolderNameICommand<object>();

        public RadAnalytics.Properties.CaptureGenericSettings Settings
        {
            get { return Properties.CaptureGenericSettings.Default; }
        }

        public string Name
        {
            get { return "General Capture Page"; }
        }
    }
}
