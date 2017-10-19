using CompleteBackup.Models.Backup.Profile;
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
    class BackupProfileRepository : ObservableObject
    {
        private const bool bRestRepositoryonStartup = false;

        static public BackupProfileRepository Instance { get; private set; } = new BackupProfileRepository();
        private BackupProfileRepository()
        {
            LoadProfile();
        }


        private ObservableCollection<BackupProfileData> BackupProfileList { get; set; }
        public BackupProfileData SelectedBackupProfile { get { return BackupProfileList?[0]; } private set { } }

        private BackupSetData _SelectedBackupSet;
        public BackupSetData SelectedBackupSet
        {
            get
            {
                if (_SelectedBackupSet == null)
                {
                    _SelectedBackupSet = BackupProfileList?[0].BackupSetList?[0];
                }

                return _SelectedBackupSet;
            }
            set
            {
                _SelectedBackupSet = value;
                OnPropertyChanged();
            }
        }

        public void LoadProfile()
        {
            try
            {
                BackupProfileList = Properties.BackupProfileRepository.Default.BackupProfileList;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error while reading backup profile\n\n {ex.Message}");
               // MessageBox.Show($"{ex.Message}", "Backup Profile", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if ((BackupProfileList == null) || bRestRepositoryonStartup)
            {
                BackupProfileList = new ObservableCollection<BackupProfileData>()
                {
                    new BackupProfileData()
                    {
                        Name = "My Sample Profile",
                        BackupSetList = new ObservableCollection<BackupSetData>()
                        {
                            new BackupSetData()
                            {
                                GUID = Guid.NewGuid(),
                                Name = "My Sample Set",
                                TargetBackupFolder = null,
                                FolderList = new ObservableCollection<string>()
                                {
                                    //"C:\\tmp\\BackupTest\\Source\\Icarus",
                                    //"C:\\tmp\\BackupTest\\Source\\ICR_Generic\\NT"
                                }
                            }
                        }
                    }
                };

                Properties.BackupProfileRepository.Default.BackupProfileList = BackupProfileList;
                SaveProfile();
            }
        }
        public void SaveProfile()
        {
            if (BackupProfileList != null)
            {                
                Properties.BackupProfileRepository.Default.Save();
            }
        }
    }
}
