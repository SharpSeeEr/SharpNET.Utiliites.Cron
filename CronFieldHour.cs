using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpNET.Utilities.Cron
{
    internal class CronFieldHour : CronFieldBase
    {
        public CronFieldHour(string value)
            : base("Hour", value, 0, 23)
        {

        }
    }
}
