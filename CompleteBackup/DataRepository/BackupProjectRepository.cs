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
    public class BackupPauseData
    {
        public string Name { get; set; }
        public int Hours { get; set; }
    }

    class BackupProjectRepository : ObservableObject
    {
        static public BackupProjectRepository Instance { get; private set; } = new BackupProjectRepository();
        private BackupProjectRepository()
        {
           LoadProjects();
        }

        public List<BackupPauseData> BackupPauseDataList { get; set; } = new List<BackupPauseData>()
        {
            new BackupPauseData() { Name = "1 Hour", Hours = 1 },
            new BackupPauseData() { Name = "2 Hours", Hours = 2 },
            new BackupPauseData() { Name = "3 Hours", Hours = 3 },
            new BackupPauseData() { Name = "6 Hours", Hours = 6 },
            new BackupPauseData() { Name = "1 Days", Hours = 24 },
            new BackupPauseData() { Name = "2 Days", Hours = 24 * 2 },
            new BackupPauseData() { Name = "3 Days", Hours = 24 * 3 },
            new BackupPauseData() { Name = "1 Week", Hours = 24 * 7 },
        };

        private ObservableCollection<BackupProjectData> BackupProjectList { get; set; }// = new ObservableCollection<BackupProjectData>();

        private BackupProjectData _SelectedBackupProject;
        public BackupProjectData SelectedBackupProject { get { return _SelectedBackupProject; } set { _SelectedBackupProject = value; OnPropertyChanged(); } }

        private const bool bRestRepositoryonStartup = false;
        public void LoadProjects()
        {
            try
            {
//                try
                {
                    Properties.BackupProjectRepositorySettings.Default.Reload();
                }
                //catch (ConfigurationErrorsException ex)
                //{ //(requires System.Configuration)
                //    string filename = ((ConfigurationErrorsException)ex.InnerException).Filename;

                //    if (MessageBox.Show("<ProgramName> has detected that your" +
                //                          " user settings file has become corrupted. " +
                //                          "This may be due to a crash or improper exiting" +
                //                          " of the program. <ProgramName> must reset your " +
                //                          "user settings in order to continue.\n\nClick" +
                //                          " Yes to reset your user settings and continue.\n\n" +
                //                          "Click No if you wish to attempt manual repair" +
                //                          " or to rescue information before proceeding.",
                //                          "Corrupt user settings",
                //                          MessageBoxButton.YesNo,
                //                          MessageBoxImage.Error) == MessageBoxResult.Yes)
                //    {
                //        File.Delete(filename);
                //        Settings.Default.Reload();
                //        // you could optionally restart the app instead
                //    }
                //    else
                //        Process.GetCurrentProcess().Kill();
                //    // avoid the inevitable crash
                //}

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
                            BackupProfileManager.CreateBackupProfileData("Sample Snapshot backup ", BackupTypeEnum.Snapshot, "home backup"),
                            BackupProfileManager.CreateBackupProfileData("Sample Incremental backup ", BackupTypeEnum.Incremental, "home backup"),
                            BackupProfileManager.CreateBackupProfileData("Sample Differential backup ", BackupTypeEnum.Differential, "home backup"),
                        }
                    }
                };

                BackupProjectList[0].CurrentBackupProfile = BackupProjectList[0].BackupProfileList[0];

//                Properties.BackupProjectRepositorySettings.Default.Reset();
                Properties.BackupProjectRepositorySettings.Default.Upgrade();
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
