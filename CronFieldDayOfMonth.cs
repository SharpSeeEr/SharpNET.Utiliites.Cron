using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpNET.Utilities.Cron
{
    internal class CronFieldDayOfMonth : CronFieldBase
    {
        public CronFieldDayOfMonth(string value)
            : base("DayOfMonth", value, 1, 31)
        {

        }
    }
}
