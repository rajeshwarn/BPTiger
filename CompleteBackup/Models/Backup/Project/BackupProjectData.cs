using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models.Backup.Project
{
    public class BackupProjectData : ObservableObject
    {
        public string Name { get; set; } = "My Project";
        public string Description { get { return Name; } set { } }
        public ObservableCollection<BackupSetData> BackupSetList { get; set; } = new ObservableCollection<BackupSetData>();
    }
}
