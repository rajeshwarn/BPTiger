using CompleteBackup.Models.Backup.History;
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

    public class RestoreFolderMenuItem : FolderMenuItem
    {
        public DateTime TimeStamp { get; set; }

        public HistoryTypeEnum? HistoryType { get; set; } = null;

        public string Image {
            get
            {
                switch (HistoryType)
                {
                    case HistoryTypeEnum.Added:
                        return "/Resources/Icons/FolderTreeView/NewItem.ico";
                    case HistoryTypeEnum.Changed:
                        return "/Resources/Icons/FolderTreeView/EditItem.ico";
                    case HistoryTypeEnum.Deleted:
                        return "/Resources/Icons/FolderTreeView/DeleteItem.ico";
                    case HistoryTypeEnum.NoChange:
                        return "/Resources/Icons/FolderTreeView/LatestItem.ico";
                    default:
                        return null;
                }
            }
            set { } }
    }

}
