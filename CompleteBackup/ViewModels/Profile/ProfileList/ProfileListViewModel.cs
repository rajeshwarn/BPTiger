using CompleteBackup.DataRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.ViewModels.Profile
{
    class ProfileListViewModel
    {
        public BackupProjectRepository Repository { get; } = BackupProjectRepository.Instance;
    }
}
