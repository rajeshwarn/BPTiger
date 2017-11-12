using CompleteBackup.DataRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.ViewModels.ExtendedControls
{
    class ProcessingSpinnerViewModel
    {
        public ProcessingSpinnerViewModel()
        {
        }

        public double SpinnerSize { get; set; } = 8 * 2; 

        public BackupProjectRepository Repository { get; set; } = BackupProjectRepository.Instance;

        static double iIntervals = Math.PI / 8;
        static double spinnerSize = 8;


        public double E_0L { get; set; } = (Math.Sin(15 * iIntervals) + 1) * spinnerSize;
        public double E_1L { get; set; } = (Math.Sin(14 * iIntervals) + 1) * spinnerSize;
        public double E_2L { get; set; } = (Math.Sin(13 * iIntervals) + 1) * spinnerSize;
        public double E_3L { get; set; } = (Math.Sin(12 * iIntervals) + 1) * spinnerSize;
        public double E_4L { get; set; } = (Math.Sin(11 * iIntervals) + 1) * spinnerSize;
        public double E_5L { get; set; } = (Math.Sin(10 * iIntervals) + 1) * spinnerSize;
        public double E_6L { get; set; } = (Math.Sin(9 * iIntervals) + 1) * spinnerSize;
        public double E_7L { get; set; } = (Math.Sin(8 * iIntervals) + 1) * spinnerSize;
        public double E_8L { get; set; } = (Math.Sin(7 * iIntervals) + 1) * spinnerSize;
        public double E_9L { get; set; } = (Math.Sin(6 * iIntervals) + 1) * spinnerSize;
        public double E_10L { get; set; } = (Math.Sin(5 * iIntervals) + 1) * spinnerSize;
        public double E_11L { get; set; } = (Math.Sin(4 * iIntervals) + 1) * spinnerSize;
        public double E_12L { get; set; } = (Math.Sin(3 * iIntervals) + 1) * spinnerSize;
        public double E_13L { get; set; } = (Math.Sin(2 * iIntervals) + 1) * spinnerSize;
        public double E_14L { get; set; } = (Math.Sin(1 * iIntervals) + 1) * spinnerSize;
        public double E_15L { get; set; } = (Math.Sin(0 * iIntervals) + 1) * spinnerSize;



        public double E_0T { get; set; } = (Math.Cos(15 * iIntervals) + 1) * spinnerSize;
        public double E_1T { get; set; } = (Math.Cos(14 * iIntervals) + 1) * spinnerSize;
        public double E_2T { get; set; } = (Math.Cos(13 * iIntervals) + 1) * spinnerSize;
        public double E_3T { get; set; } = (Math.Cos(12 * iIntervals) + 1) * spinnerSize;
        public double E_4T { get; set; } = (Math.Cos(11 * iIntervals) + 1) * spinnerSize;
        public double E_5T { get; set; } = (Math.Cos(10 * iIntervals) + 1) * spinnerSize;
        public double E_6T { get; set; } = (Math.Cos(9 * iIntervals) + 1) * spinnerSize;
        public double E_7T { get; set; } = (Math.Cos(8 * iIntervals) + 1) * spinnerSize;
        public double E_8T { get; set; } = (Math.Cos(7 * iIntervals) + 1) * spinnerSize;
        public double E_9T { get; set; } = (Math.Cos(6 * iIntervals) + 1) * spinnerSize;
        public double E_10T { get; set; } = (Math.Cos(5 * iIntervals) + 1) * spinnerSize;
        public double E_11T { get; set; } = (Math.Cos(4 * iIntervals) + 1) * spinnerSize;
        public double E_12T { get; set; } = (Math.Cos(3 * iIntervals) + 1) * spinnerSize;
        public double E_13T { get; set; } = (Math.Cos(2 * iIntervals) + 1) * spinnerSize;
        public double E_14T { get; set; } = (Math.Cos(1 * iIntervals) + 1) * spinnerSize;
        public double E_15T { get; set; } = (Math.Cos(0 * iIntervals) + 1) * spinnerSize;
    }
}
