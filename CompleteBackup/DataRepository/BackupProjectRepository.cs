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
        static public BackupProjectRepository Instance { get; private set; } = new BackupProjectRepository();
        private BackupProjectRepository()
        {
            LoadProjects();
        }

        private ObservableCollection<BackupProjectData> BackupProjectList { get; set; } = new ObservableCollection<BackupProjectData>();

        private BackupProjectData _SelectedBackupProject;
        public BackupProjectData SelectedBackupProject { get { return _SelectedBackupProject; } set { _SelectedBackupProject = value; OnPropertyChanged(); } }

        private const bool bRestRepositoryonStartup = false;
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
                        Name = "Backup Profiles",
                        BackupProfileList = new ObservableCollection<BackupProfileData>()
                        {
                            new BackupProfileData()
                            {
                                BackupType =  BackupTypeEnum.Full,
                                Name = "Sample Backup Profile",
                                Description = "home backup",
                                FolderList = new ObservableCollection<FolderData>()
                                {
                                    //empty folder list
                                },
                            },
                            new BackupProfileData()
                            {
                                BackupType =  BackupTypeEnum.Full,
                                Name = "Sample Profile2",
                                Description = "home backup",
                                FolderList = new ObservableCollection<FolderData>()
                                {
                                    //empty folder list
                                },
                            },
                            new BackupProfileData()
                            {
                                BackupType =  BackupTypeEnum.Full,
                                Name = "Sample Profile3",
                                Description = "home backup",
                                FolderList = new ObservableCollection<FolderData>()
                                {
                                    //empty folder list
                                },
                            },
                        }
                    }
                };

                BackupProjectList[0].CurrentBackupProfile = BackupProjectList[0].BackupProfileList[0];

                Properties.BackupProjectRepositorySettings.Default.Reset();
                Properties.BackupProjectRepositorySettings.Default.BackupProjectList = BackupProjectList;
                SaveProject();            
            }

            SelectedBackupProject = BackupProjectList[0];
        }
        public void SaveProject()
        {
            if (BackupProjectList != null)
            {
                Properties.BackupProjectRepositorySettings.Default.BackupProjectList = BackupProjectList;
                Properties.BackupProjectRepositorySettings.Default.Save();
            }
        }
    }
}
