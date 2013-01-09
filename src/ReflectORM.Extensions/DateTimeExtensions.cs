using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlTypes;

namespace ReflectORM.Extensions
{
    public static class DateTimeExtensions
    {
        public static int MonthSpan(this DateTime? thisValue, DateTime value)
        {
            return DateTimeExtensions.MonthSpan(thisValue.Value, value);
        }

        public static int MonthSpan(this DateTime thisValue, DateTime value)
        {
            return 12 * (thisValue.Year - value.Year) + (thisValue.Month - value.Month);
        }

        public static string GetMonthName(this DateTime value)
        {
            return string.Format("{0:MMMM}", value);
        }

        public static string GetMonthAndYear(this DateTime value)
        {
            return string.Format("{0:MMMM} {0:yyyy}", value);
        }

        public static DateTime GetFirstDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }

        public static DateTime GetLastDayOfMonth(this DateTime value)
        {
            DateTime start = value.GetFirstDayOfMonth();
            return start.AddMonths(1).AddDays(-1);
        }

        public static DateTime GetFirstDayOfYear(this DateTime value)
        {
            return new DateTime(value.Year, 1, 1);
        }

        public static DateTime GetLastDayOfYear(this DateTime value)
        {
            return new DateTime(value.Year, 12, 31);
        }

        public static string GetTimeZone(this DateTime dateTime)
        {
            if (dateTime.IsDaylightSavingTime())
                return "BST";
            else
                return "GMT";
        }

        /// <summary>
        /// Gets a relative date time string from today.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public static string GetRelativeDateTimeString(this DateTime dateTime)
        {
            DateTime cutOff = DateTime.Now.AddHours(1);

            //should set the timespan to be just before midnight the day before yesterday.
            return GetRelativeDateTimeString(dateTime, new TimeSpan(1, cutOff.Hour, cutOff.Minute, cutOff.Second));
        }

        /// <summary>
        /// Gets the relative date time string.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="cutOff">The time span before just the date is returned, defaults to 2 days.</param>
        /// <returns></returns>
        public static string GetRelativeDateTimeString(this DateTime dateTime, TimeSpan cutOff)
        {
            if (dateTime == DateTime.MinValue || dateTime == SqlDateTime.MinValue.Value)
                return string.Empty;

            TimeSpan span = DateTime.Now - dateTime;

            if (cutOff != TimeSpan.Zero && span >= cutOff)
                return string.Format("{0} {1}", dateTime.ToString("dd/MM/yyyy HH:mm"), dateTime.GetTimeZone());

            if (span.Days > 365)
            {
                var yearDiff = DateTime.Now.Year - dateTime.Year;
                var monthDiff = DateTime.Now.Month - dateTime.Month;

                if (monthDiff == 0 && yearDiff == 1)
                    return "about a year ago";
                else if (monthDiff == 0 || (monthDiff > 0 && yearDiff > 9))
                    return string.Format("about {0} years ago", yearDiff);
                else if (monthDiff == 1 && yearDiff == 1)
                    return string.Format("about 1 year, 1 month ago");
                else if (monthDiff > 0 && yearDiff == 1)
                    return string.Format("about 1 year, {0} months ago", monthDiff);
                else if (monthDiff == 1)
                    return string.Format("about {0} years, 1 month ago", yearDiff);
                else if (monthDiff > 0)
                    return string.Format("about {0} years, {1} months ago", yearDiff, monthDiff);
                else if (monthDiff == -11 && yearDiff == 2)
                    return string.Format("about 1 year, 1 month ago");
                else if (monthDiff < 0 && yearDiff == 2)
                    return string.Format("about 1 year, {0} months ago", 12 + monthDiff);
                else if (monthDiff < 0 && yearDiff > 9)
                    return string.Format("about {0} years ago", yearDiff - 1);
                else if (monthDiff == -11)
                    return string.Format("about {0} years, 1 month ago", yearDiff - 1);
                else if (monthDiff < 0)
                    return string.Format("about {0} years, {1} months ago", yearDiff - 1, 12 + monthDiff);
            }

            if (span.Days > 28)
            {
                var monthSpan = DateTime.Now.MonthSpan(dateTime);
                if (monthSpan == 1)
                    return string.Format("about a month ago");
                else
                    return string.Format("about {0} months ago", monthSpan);
            }

            if (span.Days > 1)
            {
                return string.Format("about {0} days ago", span.Days);
            }
            else
            {
                if (span.Days == 1)
                {
                    return string.Format("yesterday at {0} ", dateTime.ToShortTimeString());
                }
                else
                {
                    if (span.Hours > 0)
                    {
                        if (span.Hours == 1)
                        {
                            return "about an hour ago ";
                        }
                        else
                        {
                            return string.Format("about {0} hours ago ", span.Hours);
                        }
                    }
                    else
                    {
                        if (span.Minutes > 0)
                        {
                            if (span.Minutes == 1)
                            {
                                return "about a minute ago ";
                            }
                            else
                            {
                                return string.Format("about {0} minutes ago ", span.Minutes);
                            }
                        }
                        else
                        {
                            return "a few seconds ago ";
                        }
                    }
                }
            }
        }
    }
}
