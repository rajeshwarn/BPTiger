using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteBackup.Models.Profile
{
    enum BackupFrequenctDayEnum
    {
        Every01Minute,
        Every05Minute,
        Every10Minute,
        Every15Minute,
        Every30Minute,
        Every01Hour,
        Every02Hour,
        Every03Hour,
        Every06Hour,
        Every12Hour,
        Every01Day,
    }

    enum BackupFrequenctWeekEnum
    {
        Every01Day,
        Every02Day,
        Every03Day,
        Every01Week,
    }
    enum BackupFrequenctMonthEnum
    {
        Every01Day,
        Every02Day,
        Every03Day,
        Every01Week,
    }

    enum BackupFrequenctRemoveDeletedItemsEnum
    {
        Every01Day,
        Every02Day,
        Every01Week,
        Every02Week,
        Every01Month,
        Every02Month,
        Every03Month,
        Every04Month,
        Every06Month,
        Every01Year,
        Never,
    }



    public class BackupProfileScheduleData
    {
        public int DeepBackupDaysRunInterval { get; set; }
        public TimeSpan DeepBackupRunTime { get; set; }

        public int BackupFrequencyLastDay { get; set; }
    }
}
