using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CompleteBackup.Utilities.Convertors
{
    public class NumberToComputerSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long lSize = (long)value;

            if ((lSize) > 1000000000000)
            {
                return (lSize / 1000000000000).ToString("###,##0.0") + " TB";
            }
            if ((lSize) > 1000000000)
            {
                return (lSize / 1000000000).ToString("###,##0.0") + " GB";
            }
            else if ((lSize) > 1000000)
            {
                return (lSize / 1000000).ToString("###,##0.0") + " MB";
            }
            else if ((lSize) > 1000)
            {
                return (lSize / 1000).ToString("###,##0.0") + " KB";
            }
            else
            {
                return (lSize).ToString("###,##0") + " B";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
