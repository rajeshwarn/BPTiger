﻿using CompleteBackup.DataRepository;
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
    /// Interaction logic for SourceBackupItemsTreeView.xaml
    /// </summary>
    public partial class SourceBackupItemsTreeView : UserControl
    {
        public SourceBackupItemsTreeView()
        {
            InitializeComponent();

            //BindingExpression binding = txTaggetFolder.GetBindingExpression(TextBox.TextProperty);
            //binding.UpdateSource();
        }


   //     private void hiddenNameTextBlock_TextChanged(object sender, TextChangedEventArgs e)
   //     {
   //         var profile = BackupProjectRepository.Instance.SelectedBackupProject?.CurrentBackupProfile;

   //         var vm = this.DataContext as SourceBackupItemsTreeViewModel;
   ////         vm.Init();
   //     }

    }
}
