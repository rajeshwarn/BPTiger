using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Profile;
using CompleteBackup.ViewModels.Profile;
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

namespace CompleteBackup.Views
{
    /// <summary>
    /// Interaction logic for MainProfileView.xaml
    /// </summary>
    public partial class MainProfileView : UserControl
    {
        public MainProfileView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var profile = BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile;
            profile?.RefreshProfileProperties();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new SelectBackupTypeWindowView().Show();
        }
    }
}
