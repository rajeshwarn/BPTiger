using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Utilities
{
    class FileFolderSizeHelper
    {
        public static string GetNumberSizeString(long lSize)
        {
            if ((lSize) > 1000000000000)
            {
                return (lSize / 1000000000000).ToString("###,##0.0") + " TBytes";

            }
            if ((lSize) > 1000000000)
            {
                return (lSize / 1000000000).ToString("###,##0.0") + " GBytes";

            }
            else if ((lSize) > 1000000)
            {
                return (lSize / 1000000).ToString("###,##0.0") + " MBytes";

            }
            else if ((lSize) > 1000)
            {
                return (lSize / 1000).ToString("###,##0.0") + " KBytes";

            }
            else
            {
                return (lSize).ToString("###,##0") + " Bytes";

            }
        }
    }
}
