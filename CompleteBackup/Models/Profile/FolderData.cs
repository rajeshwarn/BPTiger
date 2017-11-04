using CompleteBackup.Models.Backup.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml.Serialization;

namespace CompleteBackup.Models.Backup.Profile
{
    public class FolderData :ObservableObject
    {
        public bool IsAvailable { get; set; } = true;
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string Name{ get; set; }

        public bool IsFolder { get; set; }

        private long m_NumberOfFiles { get; set; }
        public long NumberOfFiles { get { return m_NumberOfFiles; } set { m_NumberOfFiles = value; OnPropertyChanged(); } }

        private long m_TotalSize { get; set; }
        public long TotalSize { get { return m_TotalSize; } set { m_TotalSize = value; OnPropertyChanged(); } }

        [XmlIgnore]
        public object Image
        {
            get
            {
                ImageSource imageSource = null;
                try
                {
                    var m_IStorage = new FileSystemStorage();
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
            private set
            {
                //m_Image = value;
            }
        }
    }
}
