using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpNET.Utilities.Cron
{
    internal class CronFieldMonth : CronFieldBase
    {
        public CronFieldMonth(string value)
            : base("Month", value, 1, 12)
        {

        }
    }
}
