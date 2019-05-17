using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpNET.Utilities.Cron
{
    internal class CronFieldDayOfWeek : CronFieldBase
    {
        public CronFieldDayOfWeek(string value)
            : base("DayOfWeek", value, 0, 6)
        {

        }
    }
}
