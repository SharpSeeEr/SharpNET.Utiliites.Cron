using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpNET.Utilities.Cron
{
    public class CronEntry
    {
        private CronFieldMinute _minutes;
        public string Minutes => _minutes.Value;

        private CronFieldHour _hours;
        public string Hours => _hours.Value;

        private CronFieldDayOfMonth _daysOfMonth;
        public string DaysOfMonth => _daysOfMonth.Value;

        private CronFieldMonth _months;
        public string Months => _months.Value;

        private CronFieldDayOfWeek _daysOfWeek;
        public string DaysOfWeek => _daysOfWeek.Value;

        private CronFieldYear _years;
        public string Years => _years.Value;

        public CronEntry(string cronString)
        {
            string[] fields = cronString.Split(' ');
            if (fields.Length != 6) throw new ArgumentException("Invalid Cron Entry");

            Init(fields[0], fields[1], fields[2], fields[3], fields[4], fields[5]);
        }

        public CronEntry(string minute, string hour, string dayOfMonth, string month, string dayOfWeek, string year)
        {
            Init(minute, hour, dayOfMonth, month, dayOfWeek, year);
        }

        private void Init(string minute, string hour, string dayOfMonth, string month, string dayOfWeek, string year)
        {
            _minutes = new CronFieldMinute(minute);
            _hours = new CronFieldHour(hour);
            _daysOfMonth = new CronFieldDayOfMonth(dayOfMonth);
            _months = new CronFieldMonth(month);
            _daysOfWeek = new CronFieldDayOfWeek(dayOfWeek);
            _years = new CronFieldYear(year);
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4} {5}",
                Minutes, Hours, DaysOfMonth, Months, DaysOfWeek, Years);
        }

        /// <summary>
        /// Calculates the next trigger date and time from now
        /// </summary>
        /// <returns></returns>
        public DateTime NextTrigger()
        {
            return NextTrigger(DateTime.UtcNow);
        }

        /// <summary>
        /// Calculates the next trigger date and time occurring
        /// <paramref name="after"/> the specified date and time.
        /// </summary>
        /// <param name="after"></param>
        /// <returns>DateTime</returns>
        public DateTime NextTrigger(DateTime after)
        {
            DateTime baseDate = after.AddMinutes(1);
            int baseMinute = baseDate.Minute;
            int baseHour = baseDate.Hour;
            int baseDay = baseDate.Day;
            int baseMonth = baseDate.Month;
            int baseYear = baseDate.Year;

            // if any Next() value is -1, it should be set to the First value, and the next greater should be incremented.
            int minute = _minutes.GetNext(baseMinute);
            if (minute == -1)
            {
                minute = _minutes.GetFirst();
                ++baseHour;
            }

            // Get the next hour value
            int hour = _hours.GetNext(baseHour);
            if (hour == -1)
            {
                // Roll forward to the next day.
                minute = _minutes.GetFirst();
                hour = _hours.GetFirst();
                baseDay++;
                // Don't need to worry about baseDay>31 case because
                // that will roll off our list in the next step.
            }
            else if (hour > baseHour)
            {
                // Original hour must not have been in the list.
                // Reset the minutes.
                minute = _minutes.GetFirst();
            }

            // Get the next day value.
            int day = _daysOfMonth.GetNext(baseDay);
            if (day == -1)
            {
                // Roll forward to the next month
                minute = _minutes.GetFirst();
                hour = _hours.GetFirst();
                day = _daysOfMonth.GetFirst();
                baseMonth++;

                // Need to worry about rolling over to the next year here
                // because we need to know the number of days in a month
                // and that is year dependent (leap year).
                if (baseMonth > 12)
                {
                    // Roll over to next year.
                    baseMonth = 1;
                    baseYear++;
                }
            }
            else if (day > baseDay)
            {
                // Original day no in the value list...reset.
                minute = _minutes.GetFirst();
                hour = _hours.GetFirst();
            }

            while (day > DateTime.DaysInMonth(baseYear, baseMonth))
            {
                // Have a value for the day that is not a valid day
                // in the current month. Move to the next month.
                minute = _minutes.GetFirst();
                hour = _hours.GetFirst();
                day = _daysOfMonth.GetFirst();
                baseMonth++;
                // This original month could not be December because
                // it can handle the maximum value of days (31). So
                // we do not have to worry about baseMonth == 13 case.
            }

            // Get the next month value.
            int month = _months.GetNext(baseMonth);
            if (month == -1)
            {
                // Roll forward to the next year.
                minute = _minutes.GetFirst();
                hour = _hours.GetFirst();
                day = _daysOfMonth.GetFirst();
                month = _months.GetFirst();
                baseYear++;
            }
            else if (month > baseMonth)
            {
                // Original month not in the value list...reset.
                minute = _minutes.GetFirst();
                hour = _hours.GetFirst();
                day = _daysOfMonth.GetFirst();
            }

            while (day > DateTime.DaysInMonth(baseYear, month))
            {
                // Have a value for the day that is not a valid day
                // in the current month. Move to the next month.
                minute = _minutes.GetFirst();
                hour = _hours.GetFirst();
                day = _daysOfMonth.GetFirst();
                month = _months.GetNext(month + 1);
                if (month == -1)
                {
                    // Roll forward to the next year.
                    //
                    minute = _minutes.GetFirst();
                    hour = _hours.GetFirst();
                    day = _daysOfMonth.GetFirst();
                    month = _months.GetFirst();
                    baseYear++;
                }
            }

            DateTime next = new DateTime(baseYear, month, day, hour, minute, 0, 0);

            // Does the date / time we found satisfy the day of the week contraint?
            if (_daysOfWeek.IsValid((int)next.DayOfWeek))
            {
                return next;
            }

            // We need to recursively look for a date in the future. Because this
            //	search resulted in a day that does not satisfy the day of week
            //	contraint, start the search on the next day.
            //
            return NextTrigger(new DateTime(baseYear, month, day, 23, 59, 0, 0));
        }

        /// <summary>
        /// Calculates the previous trigger date and time occurring
        /// <paramref name="before"/> the specified date and time.
        /// </summary>
        /// <param name="before"></param>
        /// <returns>DateTime</returns>
        public DateTime PrevTrigger(DateTime before)
        {
            DateTime baseDate = before.AddMinutes(-1);
            int baseMinute = baseDate.Minute;
            int baseHour = baseDate.Hour;
            int baseDay = baseDate.Day;
            int baseMonth = baseDate.Month;
            int baseYear = baseDate.Year;

            // if any Next() value is -1, it should be set to the First value, and the next greater should be incremented.
            int minute = _minutes.GetPrev(baseMinute);
            if (minute == -1)
            {
                minute = _minutes.GetLast();
                --baseHour;
            }

            // Get the next hour value
            int hour = _hours.GetPrev(baseHour);
            if (hour == -1)
            {
                // Roll back to the previous day.
                minute = _minutes.GetLast();
                hour = _hours.GetLast();
                --baseDay;
                if (baseDay < 1)
                {
                    // Roll back to previous month
                    --baseMonth;
                    if (baseMonth < 1)
                    {
                        baseMonth = 12;
                        --baseYear;
                    }
                    baseDay = _daysOfMonth.GetPrev(DateTime.DaysInMonth(baseYear, baseMonth));
                }
            }
            else if (hour < baseHour)
            {
                // Original hour must not have been in the list.
                // Reset the minutes.
                minute = _minutes.GetLast();
            }

            // Get the previous day value.
            int day = _daysOfMonth.GetPrev(baseDay);
            if (day == -1)
            {
                // Roll back to the previous month
                minute = _minutes.GetLast();
                hour = _hours.GetLast();
                //day = _daysOfMonth.Last();
                --baseMonth;

                // Need to worry about rolling over to the next year here
                // because we need to know the number of days in a month
                // and that is year dependent (leap year).
                if (baseMonth < 1)
                {
                    // Roll over to previous year.
                    baseMonth = 12;
                    --baseYear;
                }
                day = _daysOfMonth.GetPrev(DateTime.DaysInMonth(baseYear, baseMonth));
            }
            else if (day < baseDay)
            {
                // Original day no in the value list...reset.
                minute = _minutes.GetLast();
                hour = _hours.GetLast();
            }

            while (day < 1)
            {
                // Have a value for the day that is not a valid day
                // in the current month. Move to the previous month.
                minute = _minutes.GetLast();
                hour = _hours.GetLast();
                --baseMonth;
                if (baseMonth < 1)
                {
                    baseMonth = 12;
                    --baseYear;
                }
                day = _daysOfMonth.GetPrev(DateTime.DaysInMonth(baseYear, baseMonth));
            }

            // Get the previous month value.
            int month = _months.GetPrev(baseMonth);
            if (month == -1)
            {
                // Roll back to previous year.
                minute = _minutes.GetLast();
                hour = _hours.GetLast();

                month = _months.GetLast();
                --baseYear;
                day = _daysOfMonth.GetPrev(DateTime.DaysInMonth(baseYear, month));
            }
            else if (month < baseMonth)
            {
                // Original month not in the value list...reset.
                minute = _minutes.GetLast();
                hour = _hours.GetLast();
                day = _daysOfMonth.GetLast();
            }

            while (day < 1)
            {
                // Have a value for the day that is not a valid day
                // in the current month. Move to the next month.
                minute = _minutes.GetLast();
                hour = _hours.GetLast();
                month = _months.GetPrev(month - 1);
                if (month == -1)
                {
                    // Roll back to the previous year.
                    //
                    month = _months.GetLast();
                    --baseYear;
                }
                day = _daysOfMonth.GetPrev(DateTime.DaysInMonth(baseYear, month));
            }

            DateTime prev = new DateTime(baseYear, month, day, hour, minute, 0, 0);

            // Does the date / time we found satisfy the day of the week contraint?
            if (_daysOfWeek.IsValid((int)prev.DayOfWeek))
            {
                return prev;
            }

            // We need to recursively look for a date in the past. Because this
            //	search resulted in a day that does not satisfy the day of week
            //	contraint, start the search on the previous day.
            return PrevTrigger(new DateTime(baseYear, month, day, 0, 0, 0, 0));
        }
    }
}
