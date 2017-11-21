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
    class DatabaseGeneralViewModel : ObservableObject, IPageViewModel
    {
        public DatabaseGeneralViewModel()
        {

        }

        //public RadAnalytics.Properties.MongoDBSettings MongoDBSettings
        //{
        //    get
        //    {
        //        return Properties.MongoDBSettings.Default;
        //    }
        //}

        //public RadAnalytics.Properties.ProcessStartSetting ProcessStartSetting
        //{
        //    get
        //    {
        //        return Properties.ProcessStartSetting.Default;
        //    }
        //}

        public string Name
        {
            get
            {
                return "Database General";
            }
        }
    }
}
