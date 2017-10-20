using CompleteBackup.Models.Backup.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CompleteBackup.Models.Backup.Profile
{
    public class BackupProfileData : ObservableObject
    {
        public Guid GUID { get; set; } = Guid.Empty;
        public string Name { get; set; } = "My Backup Profile";
        public string Description { get { return Name; } set { } }

        IStorageInterface m_IStorage = new FileSystemStorage();

        public ObservableCollection<string> FolderList { get; set; } = new ObservableCollection<string>();

        [XmlIgnore]
        public bool IsValidSetData
        {
            get
            {
                bool bValidSet = false;
                var subdirectoryList = GetDirectoriesNames(_TargetBackupFolder);

                foreach (string subDirectory in subdirectoryList)
                {
                    string newPath = m_IStorage.Combine(_TargetBackupFolder, subDirectory);
                    FileAttributes attr = m_IStorage.GetFileAttributes(newPath);

                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        bValidSet |= subDirectory.StartsWith(GUID.ToString());
                    }
                }

                return bValidSet;
            }

            private set { }
        }

        private string _TargetBackupFolder;
        public string TargetBackupFolder { get { return _TargetBackupFolder; } set { _TargetBackupFolder = value; OnPropertyChanged(); } }

        public List<string> GetDirectoriesNames(string path)
        {

            //Process directories
            string[] sourceSubdirectoryEntries = m_IStorage.GetDirectories(path);
            var sourceSubdirectoryEntriesList = new List<string>();
            if (sourceSubdirectoryEntries != null)
            {
                if (sourceSubdirectoryEntriesList != null)
                {
                    foreach (var entry in sourceSubdirectoryEntries)
                    {
                        sourceSubdirectoryEntriesList.Add(m_IStorage.GetFileName(entry));
                    }
                }
            }

            return sourceSubdirectoryEntriesList;
        }


    }
}
