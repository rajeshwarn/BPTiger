using CompleteBackup.Models.Backup.Storage;
using Newtonsoft.Json;
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
        private bool m_IsAvailable = true;
        public bool IsAvailable { get { return m_IsAvailable; } set { m_IsAvailable = value; OnPropertyChanged(); OnPropertyChanged("Image");} }

        private string m_Path { get; set; }
        public string Path { get { return m_Path; } set { m_Path = value; OnPropertyChanged();} }

        public string RelativePath { get; set; }
        private string m_Name { get; set; }
        public string Name { get { return m_Name; } set { m_Name = value; OnPropertyChanged(); } }

        public bool IsFolder { get; set; }

        private long m_NumberOfFiles { get; set; }
        public long NumberOfFiles { get { return m_NumberOfFiles; } set { m_NumberOfFiles = value; OnPropertyChanged(); } }

        private long m_TotalSize { get; set; }
        public long TotalSize { get { return m_TotalSize; } set { m_TotalSize = value; OnPropertyChanged(); } }

        [JsonIgnore]
        [XmlIgnore]
        public object Image
        {
            get
            {
                ImageSource imageSource = null;
                try
                {
                    if (IsAvailable)
                    {
                        var m_IStorage = new FileSystemStorage();
                        var icon = m_IStorage.ExtractIconFromPath(Path);
                        imageSource = Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            System.Windows.Int32Rect.Empty,
                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    }
                    else
                    {
                        return "/Resources/Icons/Alert.ico";
                    }
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
