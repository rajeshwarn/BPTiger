using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models.Backup.Storage
{
    class Win32LongPathPath
    {
        public const int MAX_PATH = 260;

        public static string GetDirectoryName(string path)
        {
            int iIndex = path.LastIndexOf('\\');

            return path.Substring(0, iIndex);
        }
    }
}
