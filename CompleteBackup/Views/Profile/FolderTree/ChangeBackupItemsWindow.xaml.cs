﻿using CompleteBackup.Models.FolderSelection;
using CompleteBackup.ViewModels;
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
using System.Windows.Shapes;

namespace CompleteBackup.Views
{
    /// <summary>
    /// Interaction logic for ChangeBackupItemsWindow.xaml
    /// </summary>
    public partial class ChangeBackupItemsWindow : Window
    {
        public ChangeBackupItemsWindow()
        {
            InitializeComponent();
        }
        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = e.OriginalSource as TreeViewItem;
            var itemList = tvi.Items;

            var vm = DataContext as ChangeBackupItemsWindowModel;
            vm.ExpandFolder(itemList);
        }

        private void FolderCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = e.OriginalSource as CheckBox;

            if (checkBox.IsChecked == null)
            {
                checkBox.IsChecked = false;
            }

            var dc = checkBox.DataContext as BackupFolderMenuItem;
            var viewModel = DataContext as ChangeBackupItemsWindowModel;
            viewModel.FolderTreeClick(dc, (bool)checkBox.IsChecked);
        }
    }
}