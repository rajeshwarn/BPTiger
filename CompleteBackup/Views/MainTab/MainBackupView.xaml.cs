using CompleteBackup.Models.backup;
using CompleteBackup.ViewModels;
using CompleteBackup.Views.MainWindow;
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
    /// Interaction logic for MainBackupView.xaml
    /// </summary>
    public partial class MainBackupView : UserControl
    {
        public MainBackupView()
        {
            InitializeComponent();
        }


        private void StartBackupButton_Click(object sender, RoutedEventArgs e)
        {
            var folderSelection = folderTree.DataContext as SourceBackupItemsTreeViewModel;

            var progressBar = GenericStatusBarView.NewInstance;
            progressBar.UpdateProgressBar("Backup starting...", 0);

            var backup = BackupFactory.CreateFullBackupTask(folderSelection.ProjectData.CurrentBackupProfile.FolderList.ToList<string>(), folderSelection.ProjectData.CurrentBackupProfile.TargetBackupFolder, progressBar);
            backup.RunWorkerAsync();
        }
    }
}
