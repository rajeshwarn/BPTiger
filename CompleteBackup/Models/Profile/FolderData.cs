using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models.Backup.Profile
{
    public class FolderData :ObservableObject
    {
        public string Path { get; set; }

        private long m_NumberOfFiles { get; set; }
        public long NumberOfFiles { get { return m_NumberOfFiles; } set { m_NumberOfFiles = value; OnPropertyChanged(); } }

        private long m_TotalSize { get; set; }
        public long TotalSize { get { return m_TotalSize; } set { m_TotalSize = value; OnPropertyChanged(); } }
    }
}
