using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models.Backup
{
    public class FileSystemWatcherItemData
    {
        public string WatchPath { get; set; }

        public DateTime Time { get; set; }
        public WatcherChangeTypes ChangeType { get; set; }
        public string OldPath { get; set; }
        public string FullPath { get; set; }
        public string Name { get; set; }

    }
}
