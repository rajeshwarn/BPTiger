using CompleteBackup.ViewModels.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models
{
    class MainWindowModel
    {
        public object CurrentPageViewModel { get; set; } = new ProfileListViewModel();
    }
}
