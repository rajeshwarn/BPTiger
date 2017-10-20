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
using System.IO;
using CompleteBackup.Models.backup;
using CompleteBackup.Views.MainWindow;
using CompleteBackup.ViewModels;

namespace CompleteBackup.Views
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();

            this.DataContext = this;

            var folderSelection = folderTree.DataContext as FolderTreeViewModel;
            //folderSelection.FolderList;// = new List<string>() { "D:\\Master Photos Catalog", "D:\\Personal", "D:\\Master Video Catalog" };

        }

        //        public string SourcePath { get; set; } = "C:\\tmp\\BackupTest\\Source";
        //      public string TargetPath { get; set; } = "C:\\tmp\\BackF:\BACKUPupTest\\Target";



        //        public string SourcePath { get; set; } = "E:\\test\\source";
        //        public string TargetPath { get; set; } = "E:\\test\\target";
        public List<string> SourcePath { get; set; } = new List<string>() { };
        public string TargetPath { get; set; } = "F:\\BACKUP";

        public long FileCount { get; set; } = 0;


        private void StartBackupButton_Click(object sender, RoutedEventArgs e)
        {
            var folderSelection = folderTree.DataContext as FolderTreeViewModel;

            var progressBar = GenericStatusBarView.NewInstance;
            progressBar.UpdateProgressBar("Backup starting...", 0);

            var backup = BackupFactory.CreateFullBackupTask(folderSelection.SetData.FolderList.ToList<string>(), folderSelection.SetData.TargetBackupFolder, progressBar);
            backup.RunWorkerAsync();
        }


    }
}
