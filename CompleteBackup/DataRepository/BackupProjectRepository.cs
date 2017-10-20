using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.Models.Backup.Project;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CompleteBackup.DataRepository
{
    class BackupProjectRepository : ObservableObject
    {
        private const bool bRestRepositoryonStartup = false;

        static public BackupProjectRepository Instance { get; private set; } = new BackupProjectRepository();
        private BackupProjectRepository()
        {
            LoadProjects();
        }


        private ObservableCollection<BackupProjectData> BackupProjectList { get; set; }
        public BackupProjectData SelectedBackupProject { get { return BackupProjectList?[0]; } private set { } }

        private BackupProfileData _SelectedBackupProfile;
        public BackupProfileData SelectedBackupProfile
        {
            get
            {
                if (_SelectedBackupProfile == null)
                {
                    _SelectedBackupProfile = BackupProjectList?[0].BackupProfileList?[0];
                }

                return _SelectedBackupProfile;
            }
            set
            {
                _SelectedBackupProfile = value;
                OnPropertyChanged();
            }
        }

        public void LoadProjects()
        {
            try
            {
                BackupProjectList = Properties.BackupProjectRepositorySettings.Default.BackupProjectList;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error while reading backup Project\n\n {ex.Message}");
               // MessageBox.Show($"{ex.Message}", "Backup Project", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if ((BackupProjectList == null) || (BackupProjectList?.Count() == 0) || bRestRepositoryonStartup)
            {
                BackupProjectList = new ObservableCollection<BackupProjectData>()
                {
                    new BackupProjectData()
                    {
                        Name = "My first Project",
                        BackupProfileList = new ObservableCollection<BackupProfileData>()
                        {
                            new BackupProfileData()
                            {
                                GUID = Guid.NewGuid(),
                                Name = "My first backup set",
                                FolderList = new ObservableCollection<string>()
                                {
                                }
                            }
                        }
                    }
                };

                Properties.BackupProjectRepositorySettings.Default.Reset();
                Properties.BackupProjectRepositorySettings.Default.BackupProjectList = BackupProjectList;
                SaveProject();            
            }
        }
        public void SaveProject()
        {
            if (BackupProjectList != null)
            {                
                Properties.BackupProjectRepositorySettings.Default.Save();
            }
        }
    }
}
