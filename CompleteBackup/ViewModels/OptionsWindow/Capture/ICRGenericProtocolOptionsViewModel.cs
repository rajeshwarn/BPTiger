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
using System.Net;

namespace RadAnalytics.ViewModels
{

    public class ICRGenericDeviceNetworkSetting : ObservableObject
    {
        public bool IsEnable { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public string IPAddress { get; set; } = Properties.ICRGenericProtocolSettings.Default.ServerIPNumber;
        public int PortNumber { get; set; } = Properties.ICRGenericProtocolSettings.Default.ServerPortNumber;

        public string IndexNumber
        {
            get
            {
                return $"{Properties.ICRGenericProtocolSettings.Default.ICRGenericDeviceNetworkSettingList.IndexOf(this) + 1}.";
            }
        }
    }

    class ICRGenericProtocolOptionsViewModel : ObservableObject, IPageViewModel
    {
        public ICRGenericProtocolOptionsViewModel()
        {
            String strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            NetworkInterfaceList = ipEntry.AddressList;

            if (Properties.ICRGenericProtocolSettings.Default.ICRGenericDeviceNetworkSettingList == null)
            {
                Properties.ICRGenericProtocolSettings.Default.ICRGenericDeviceNetworkSettingList = new ObservableCollection<ICRGenericDeviceNetworkSetting>() { new ICRGenericDeviceNetworkSetting() };            
            }
        }

        public ICommand RestartICRGenericProtocolEmulationCommand { get; private set; } = new RestartICRGenericProtocolEmulationICommand<object>();
        public ICommand AddNewICRGenericDeviceSettingCommand { get; private set; } = new AddNewICRGenericDeviceSettingICommand<object>();
        public ICommand DeleteICRGenericDeviceSettingCommand { get; private set; } = new DeleteICRGenericDeviceSettingICommand<object>();

        public ICommand SelectFolderNameCommand { get; private set; } = new SelectFolderNameICommand<object>();

        public RadAnalytics.Properties.ICRGenericProtocolSettings Settings
        {
            get
            {
                return Properties.ICRGenericProtocolSettings.Default;
            }
        }


        public IPAddress[] NetworkInterfaceList { get; set; }


        public string Name
        {
            get
            {
                return "ICR Generic Config Page";
            }
        }
    }
}
