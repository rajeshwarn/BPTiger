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

        //protected ImageSource m_Image;
        //public ImageSource Image2
        //{
        //    get { return m_Image; }
        //    set { m_Image = value; OnPropertyChanged(); }
        //}

//        m_Image = GetImageSource(path),

//        m_Image = GetImageSource(path),



        //public ImageSource Image
        //{
        //    get
        //    {
        //        ImageSource imageSource = null;
        //        try
        //        {
        //            var icon = m_ ExtractFromPath(Path);

        //            imageSource = Imaging.CreateBitmapSourceFromHIcon(
        //                icon.Handle,
        //                System.Windows.Int32Rect.Empty,
        //                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        //        }
        //        catch (Exception ex)
        //        {
        //            Trace.WriteLine($"Failed to get Icon {Path}\n{ex.Message}");
        //        }

        //        return imageSource;
        //    }
        //    set { }
        //}
    }
}
