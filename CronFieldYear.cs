using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpNET.Utilities.Cron
{
    internal class CronFieldYear : CronFieldBase
    {
        public CronFieldYear(string value)
            : base("Year", value, 2000, 2100)
        {

        }
    }
}
