using CompleteBackup.DataRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.ViewModels.MainWindow
{
    class MainWindowViewModel
    {
        public MainWindowViewModel()
        {

        }
        public BackupProfileRepository Profile { get; set; } = BackupProfileRepository.Instance;
        public string TEXT { get; set; } = "guy test";
    }
}
