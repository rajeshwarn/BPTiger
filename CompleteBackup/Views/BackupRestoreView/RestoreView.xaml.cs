using CompleteBackup.DataRepository;
using CompleteBackup.Models.Backup.History;
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
    /// Interaction logic for CommonItemsSelectionView.xaml
    /// </summary>
    public partial class RestoreView : UserControl
    {
        public RestoreView()
        {
            InitializeComponent();
        }
    
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var vm = DataContext as GenericBackupItemsSelectionViewModel;
            vm.UpdateCurrentProfileChange();
        }

        private void HistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0];
                var vm = DataContext as RestoreItemsSelectionViewModel;

                var historyItem = item as BackupSessionHistory;
                vm.SelectedHistoryItem = historyItem;

                var selectHistoryItemsViewModel = viewSelectHistoryItemsView.DataContext as SelectHistoryItemsViewModel;
                selectHistoryItemsViewModel.SelectedHistoryItem = historyItem;
            }
        }
    }
}
