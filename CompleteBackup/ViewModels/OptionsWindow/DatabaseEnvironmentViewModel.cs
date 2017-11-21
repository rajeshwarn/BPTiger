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
using RadAnalytics.ViewModels;

namespace RadAnalytics.ViewModels
{
    class DatabaseEnvironmentViewModel : ObservableObject, IPageViewModel
    {
        public DatabaseEnvironmentViewModel()
        {
        }

        public ICommand SelectFolderNameCommand { get; private set; } = new SelectFolderNameICommand<object>();

        public RadAnalytics.Properties.MongoDBSettings MongoDBSettings
        {
            get
            {
                return Properties.MongoDBSettings.Default;
            }
        }

        public string Name
        {
            get
            {
                return "Database Environment Page";
            }
        }
    }
}
