using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpNET.Utilities.Cron
{
    internal class CronFieldMinute : CronFieldBase
    {
        public CronFieldMinute(string value)
            : base("Minute", value, 0, 59)
        {

        }
    }
}
