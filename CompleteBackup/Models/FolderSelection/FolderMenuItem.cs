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
    public class FolderMenuItem : ObservableObject
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string RelativePath { get; set; }

        public bool IsFolder { get; set; } = false;
        public HistoryTypeEnum? HistoryType { get; set; } = null;

        public bool IsSelectable { get; set; } = true;

        public bool? _Selected = false;
        public bool? Selected { get { return _Selected; } set { _Selected = value; OnPropertyChanged(); } }

        public FolderMenuItem ParentItem { get; set; }
        public ObservableCollection<FolderMenuItem> ChildFolderMenuItems { get; set; } = new ObservableCollection<FolderMenuItem>();


        static IStorageInterface m_IStorage = new FileSystemStorage();

        //object m_Image = null;
        public object Image
        {
            get
            {
                if (HistoryType == null || (HistoryType == HistoryTypeEnum.NoChange))
                {
                    ImageSource imageSource = null;
                    try
                    {
                        var icon = m_IStorage.ExtractIconFromPath(Path);
                        imageSource = Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            System.Windows.Int32Rect.Empty,
                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Failed to get Icon {Path}\n{ex.Message}");
                    }

                    return imageSource;

                }
                else
                {
                    switch (HistoryType)
                    {
                        case HistoryTypeEnum.Added:
                            return IsFolder ? "/Resources/Icons/FolderTreeView/NewFolder.ico" : "/Resources/Icons/FolderTreeView/NewItem.ico";

                        case HistoryTypeEnum.Changed:
                            return "/Resources/Icons/FolderTreeView/EditItem.ico";

                        case HistoryTypeEnum.Deleted:
                            return IsFolder ? "/Resources/Icons/FolderTreeView/DeleteFolder.ico" : "/Resources/Icons/FolderTreeView/DeleteItem.ico";

                        case HistoryTypeEnum.NoChange:
                            return "/Resources/Icons/FolderTreeView/LatestItem.ico";

                        default:
                            return "/Resources/Icons/FolderTreeView/LatestItem.ico";
                    }
//                    return m_Image;// ;
                }
            }
            private set
            {
                //m_Image = value;
            }
        }
    }
}
