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

    //public class BackupFolderMenuItem : FolderMenuItem
    //{
    //    public FileAttributes Attributes { get; set; }
    //}


    public class BackupFolderMenuItem : FolderMenuItem
    {

        public FileAttributes Attributes { get; set; } = 0;

        public ImageSource Image
        {
            get
            {
                Trace.WriteLine($"XXXXXXXXXXXXXXX- IMAGE");

                return null;
            }
        }

        public ImageSource MenuItemImage
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
            set { }
        }

        static IStorageInterface  m_IStorage = new FileSystemStorage();
    }
}
