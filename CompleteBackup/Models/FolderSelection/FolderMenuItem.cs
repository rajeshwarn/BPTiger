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

        public bool m_IsExpanded = false;
        public bool IsExpanded { get { return m_IsExpanded; } set { m_IsExpanded = value; OnPropertyChanged(); } }

        public bool IsSelectable { get; set; } = true;

        public bool? m_Selected = false;
        public bool? Selected { get { return m_Selected; } set { m_Selected = value; OnPropertyChanged(); } }

        public FolderMenuItem ParentItem { get; set; }
        public ObservableCollection<FolderMenuItem> ChildFolderMenuItems { get; set; } = new ObservableCollection<FolderMenuItem>();


        static IStorageInterface m_IStorage = new FileSystemStorage();

        public object Image
        {
            get
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
            private set { }
        }



        
        public object HistoryTypeImage
        {
            get
            {
                switch (HistoryType)
                {
                    case HistoryTypeEnum.Added:
                        return "/Resources/Icons/FolderTreeView/Add.ico";

                    case HistoryTypeEnum.Changed:
                        return "/Resources/Icons/FolderTreeView/Update.ico";

                    case HistoryTypeEnum.Deleted:
                        return "/Resources/Icons/FolderTreeView/Delete.ico";

                    case HistoryTypeEnum.NoChange:
                        return null;

                    default:
                        return null;
                }
            }
            private set { }
        }
    }
}
