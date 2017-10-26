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
        public FolderMenuItem()
        {
            this.SourceBackupItems = new ObservableCollection<FolderMenuItem>();
        }

        public string Name { get; set; }
        public string Path { get; set; }

        public DateTime TimeStamp { get; set; }


        public FileAttributes Attributes { get; set; }

        private static Icon ExtractFromPath(string path)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(
                path,
                0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | SHGFI_LARGEICON);
            return System.Drawing.Icon.FromHandle(shinfo.hIcon);
        }
        //Struct used by SHGetFileInfo function
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x000000001;
        public ImageSource Image {
            get
            {
                ImageSource imageSource = null;
                //                if (((Attributes & FileAttributes.Archive) == FileAttributes.Archive) &&
                //                if (!IsFolder && !IsSystemFile)
                //if (!IsFolder)
                try
                {
                    var icon = ExtractFromPath(Path);

//                    var icon = Icon.ExtractAssociatedIcon(Path);
                        imageSource = Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        System.Windows.Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                }
                catch( Exception ex)
                {
                    Trace.WriteLine($"Failed to get Icon {Path}\n{ex.Message}");
                }

                return imageSource;
            }
            set { }
        }
        public bool IsSystemFile
        {
            get
            {
                bool bSystem = (((Attributes & FileAttributes.System) == FileAttributes.System) ||
                                ((Attributes & FileAttributes.Directory) == FileAttributes.Directory));
//                ((Attributes & FileAttributes.Hidden) != FileAttributes.Hidden));

                return bSystem;
            }
            set { }
        }

        public bool IsFolder { get; set; } = false;


        public bool? _Selected = false;
        public bool? Selected { get { return _Selected; } set { _Selected = value; OnPropertyChanged(); } }

        public FolderMenuItem ParentItem { get; set; }
        public ObservableCollection<FolderMenuItem> SourceBackupItems { get; set; }
    }

}
