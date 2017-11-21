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
using System.IO;

namespace RadAnalytics.ViewModels
{
    class RadDataCaptureViewModel : ObservableObject, IPageViewModel
    {
        public RadDataCaptureViewModel()
        {
            SetJagSpyProxyManuallyCommand  = new SetJagSpyProxyManuallyICommand<object>(this);
        }

        public ICommand SelectFolderNameCommand { get; private set; } = new SelectFolderNameICommand<object>();
        public ICommand SetJagSpyProxyManuallyCommand { get; private set; }

        public string JagProxyDLLVersion
        {
            get
            {
                string dllName = null;
                try
                {
                    dllName = new DirectoryInfo(Properties.JagSettings.Default.JagProxyDLLPath).Name;
                }
                catch { }

                return dllName;
            }
            set { }
        }

        public void UpdateJagProxyDLLVersion()
        {
            OnPropertyChanged("JagProxyDLLVersion");
        }

        public RadAnalytics.Properties.JagSettings JagSettings
        {
            get
            {
                return Properties.JagSettings.Default;
            }
        }
        public string JagSpyProxyDLLName
        {
            get
            {
                return JagInterfaceBase.DllFilePath;
            }
        }

        public string Name
        {
            get
            {
                return "RadData Capture Page";
            }
        }
    }
}
