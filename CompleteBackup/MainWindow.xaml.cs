using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup;
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

namespace CompleteBackup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var profile = BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile;
            profile?.UpdateProfileProperties();

            CBFileSystemWatcher.Run(@"E:\test2\source");

        }

    private void BackupTypeRibbonGallery_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
       // MessageBox.Show(rcmbCategory.SelectionBoxItem.ToString());
    }
}
}
