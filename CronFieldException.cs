using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpNET.Utilities.Cron
{
    public class CronFieldException : Exception
    {
        public CronFieldException(string message = "")
            : base(message)
        {

        }
    }
}
