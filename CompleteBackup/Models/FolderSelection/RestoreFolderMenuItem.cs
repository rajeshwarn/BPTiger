using CompleteBackup.Models.Backup.History;
using CompleteBackup.Models.Backup.Storage;
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



        // public ImageSource Image { get; set; }
        //public string Image {
        //    get
        //    {
        //        if (IsFolder)
        //        {
        //            switch (HistoryType)
        //            {
        //                case HistoryTypeEnum.Added:
        //                    return "/Resources/Icons/FolderTreeView/NewFolder.ico";
        //                case HistoryTypeEnum.Deleted:
        //                    return "/Resources/Icons/FolderTreeView/DeleteFolder.ico";
        //                case HistoryTypeEnum.Changed:
        //                case HistoryTypeEnum.NoChange:
        //                default:
        //                    return "/Resources/Icons/FolderTreeView/Folder.ico";
        //            }
        //        }
        //        else
        //        {
        //            switch (HistoryType)
        //            {
        //                case HistoryTypeEnum.Added:
        //                    return "/Resources/Icons/FolderTreeView/NewItem.ico";
        //                case HistoryTypeEnum.Changed:
        //                    return "/Resources/Icons/FolderTreeView/EditItem.ico";
        //                case HistoryTypeEnum.Deleted:
        //                    return "/Resources/Icons/FolderTreeView/DeleteItem.ico";
        //                case HistoryTypeEnum.NoChange:
        //                    return "/Resources/Icons/FolderTreeView/Item.ico";
        //                default:

        //                    return null;
        //            }
        //        }
        //    }
        //    set { } }
    }

}
