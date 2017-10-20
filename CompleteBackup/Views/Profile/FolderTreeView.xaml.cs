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
    /// Interaction logic for FolderTreeView.xaml
    /// </summary>
    public partial class FolderTreeView : UserControl
    {
        public FolderTreeView()
        {
            InitializeComponent();

            var viewModel = DataContext as FolderTreeViewModel;
            foreach (var item in viewModel.RootFolderItemList)
            {
                trvFolderTreeView.Items.Add(item);
            }
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            var itemSource = e.OriginalSource;
            TreeViewItem tvi = e.OriginalSource as TreeViewItem;
            var itemList = tvi.Items;

            var vm = DataContext as FolderTreeViewModel;
            vm.ExpandFolder(itemList);
        }



        private void FolderCheckBox_Click(object sender, RoutedEventArgs e)
        {

            var checkbox = e.OriginalSource as CheckBox;
            var dc = checkbox.DataContext as FolderMenuItem;

            if (checkbox.IsChecked == null)
            {
                checkbox.IsChecked = false;
            }

            var viewModel = DataContext as FolderTreeViewModel;
            viewModel.FolderTreeClick(dc, (bool)checkbox.IsChecked);
        }

        private void txTaggetFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
       //     var vm = this.DataContext as FolderTreeViewModel;
       //     vm.ProfileData.UpdateProfileTargetFolderStatus();

            BindingExpression binding = txTaggetFolder.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
        }
    }
}
