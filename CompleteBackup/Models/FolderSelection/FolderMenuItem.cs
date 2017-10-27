using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;

namespace CompleteBackup.Models.FolderSelection
{

    public class FolderMenuItem : ObservableObject
    {
        public string Name { get; set; }
        public string Path { get; set; }



        public bool IsFolder { get; set; } = false;


        public bool? _Selected = false;
        public bool? Selected { get { return _Selected; } set { _Selected = value; OnPropertyChanged(); } }

        public FolderMenuItem ParentItem { get; set; }
        public ObservableCollection<FolderMenuItem> SourceBackupItems { get; set; } = new ObservableCollection<FolderMenuItem>();
    }

}
