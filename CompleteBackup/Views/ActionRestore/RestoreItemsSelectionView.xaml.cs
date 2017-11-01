using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.Storage;
using CompleteBackup.Models.FolderSelection;
using CompleteBackup.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for RestoreItemsSelectionView.xaml
    /// </summary>
    public partial class RestoreItemsSelectionView : UserControl
    {
        public RestoreItemsSelectionView()
        {
            InitializeComponent();

            //var vm = this.DataContext as RestoreItemsSelectionViewModel;
            //vm.ProjectData.CurrentBackupProfile.TargetRestoreFolder = vm.ProjectData.CurrentBackupProfile.TargetRestoreFolder;
        }
    }
}
