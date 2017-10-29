using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.ViewModels
{
    class MainBackupViewModel
    {
        public BackupItemsSelectionViewModel BackupItemsSelectionViewModel { get; set; } = new BackupItemsSelectionViewModel();
    }
}
