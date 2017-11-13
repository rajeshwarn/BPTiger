using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CompleteBackup.Views.Profile
{
    /// <summary>
    /// Interaction logic for ProfileListView.xaml
    /// </summary>
    public partial class ProfileListView : UserControl
    {
        public ProfileListView()
        {
            InitializeComponent();
        }

        private void ListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            //var listBox = sender as ListBox;
            //var item = e.Source as BackupProfileData;
            //var item2 = listBox.SelectedItem as BackupProfileData;
            //BackupProjectRepository.Instance.SelectedBackupProject.CurrentBackupProfile = item;
        }
    }
}
