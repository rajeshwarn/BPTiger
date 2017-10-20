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
        [Flags]
        public enum ProfileTargetFolderStatusEnum
        {
            AssosiatedWithThisProfile, //no error looks good
            AssosiatedWithADifferentProfile,
            EmptyFolderNoProfile,
            NonEmptyFolderNoProfile,
            CoccuptedOrNotRecognizedProfile,
            InvalidTargetPath
        }

        public Guid GUID { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "My Backup Profile";
        public string Description { get { return Name; } set { } }

        IStorageInterface m_IStorage = new FileSystemStorage();

        public ObservableCollection<string> FolderList { get; set; } = new ObservableCollection<string>();

        [XmlIgnore]
        public bool IsValidProfileFolder { get { return GetProfileTargetFolderStatus(_TargetBackupFolder) != ProfileTargetFolderStatusEnum.AssosiatedWithThisProfile; } }

        [XmlIgnore]
        public ProfileTargetFolderStatusEnum ProfileTargetFolderStatus { get { return GetProfileTargetFolderStatus(_TargetBackupFolder); } }

        //private ProfileTargetFolderStatusEnum m_ProfileTargetFolderStatus = ProfileTargetFolderStatusEnum.InvalidTargetPath;
        public ProfileTargetFolderStatusEnum GetProfileTargetFolderStatus(string path)
        {
            ProfileTargetFolderStatusEnum profileStatus;

            if (!m_IStorage.DirectoryExists(path))
            {
                profileStatus = ProfileTargetFolderStatusEnum.InvalidTargetPath;
            }
            else
            {
                var subdirectoryList = GetDirectoriesNames(path);

                if (subdirectoryList == null || subdirectoryList.Count() == 0)
                {
                    profileStatus = ProfileTargetFolderStatusEnum.EmptyFolderNoProfile;
                }
                else
                {
                    int iMatchCount = 0;
                    foreach (string subDirectory in subdirectoryList)
                    {
                        string newPath = m_IStorage.Combine(path, subDirectory);
                        FileAttributes attr = m_IStorage.GetFileAttributes(newPath);

                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            if (subDirectory.StartsWith(GUID.ToString()))
                            {
                                iMatchCount++;
                            }
                        }
                    }

                    if (iMatchCount == subdirectoryList.Count)
                    {
                        profileStatus = ProfileTargetFolderStatusEnum.AssosiatedWithThisProfile;
                    }
                    else if (iMatchCount == 0)
                    {
                        profileStatus = ProfileTargetFolderStatusEnum.NonEmptyFolderNoProfile;
                    }
                    else 
                    {
                        profileStatus = ProfileTargetFolderStatusEnum.CoccuptedOrNotRecognizedProfile;
                    }
                }
            }

            //if (m_ProfileTargetFolderStatus != profileStatus)
            //{
            //    m_ProfileTargetFolderStatus = profileStatus;

            //    OnPropertyChanged("ProfileTargetFolderStatus");
            //    OnPropertyChanged("IsValidProfileFolder");

            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
            return profileStatus;
        }

        private string _TargetBackupFolder;
        public string TargetBackupFolder { get { return _TargetBackupFolder; } set  { _TargetBackupFolder = value; OnPropertyChanged(); } }

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
