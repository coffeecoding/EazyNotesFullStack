using System;

namespace EazyNotes.Common
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts time to string as 'YYYY-MM-DD HH-MM-SSZ'. Note the trailing 'Z'.
        /// Does NOT convert the time to UTC time!
        /// </summary>
        public static string ToISO8601UTCString(this DateTime time)
        {
            return $"{time.Year.ToString().PadLeft(4, '0')}-{time.Month.ToString().PadLeft(2, '0')}-{time.Day.ToString().PadLeft(2, '0')}T{time.Hour.ToString().PadLeft(2, '0')}:{time.Minute.ToString().PadLeft(2, '0')}:{time.Second.ToString().PadLeft(2, '0')}Z";
        }
        
        public static bool IsEqual(this DateTime first, DateTime second)
        {
            return first.ToUniversalTime().Equals(second.ToUniversalTime());
        }

        public static string ToFileSystemFriendlyString(this DateTime time)
        {
            var utc = time.ToUniversalTime();
            return $"{utc.Year.ToString()[2..]}{utc.Month.ToString().PadLeft(2,'0')}{utc.Day.ToString().PadLeft(2,'0')}-{utc.Hour.ToString().PadLeft(2, '0')}-{utc.Minute.ToString().PadLeft(2, '0')}-{utc.Second.ToString().PadLeft(2, '0')}";
        }

        /// <summary>
        /// Converts time to string as 'YYYY-MM-DD HH-MM-SSZ'. Note the trailing 'Z'.
        /// Does NOT convert the time to UTC time!
        /// </summary>
        public static string ToISO8601UTCString(this DateTime? time)
        {
            if (time.HasValue)
                return ToISO8601UTCString(time.Value);
            else return null;
        }

        /// <summary>
        /// Returns "'...'" if not null, else returns "null", thus can be plugged directly into SQL string.
        /// </summary>
        public static string ToISO8601UTCSQLValueString(this DateTime? time)
        {
            if (time.HasValue)
                return $"'{ToISO8601UTCString(time.Value)}'";
            else return "null";
        }

        public static string ToMySQLDateTimeLiteral(this DateTime time)
        {
            return ToISO8601UTCString(time).Replace("T", " ").Replace("Z", " ");
        }

        public static string ToMySQLDateTimeLiteral(this DateTime? time)
        {
            if (time.HasValue)
                return time.Value.ToMySQLDateTimeLiteral();
            return null;
        }

        public static bool IsEqual(this DateTime? first, DateTime? second)
        {
            return first.GetValueOrDefault(DateTime.MinValue).ToUniversalTime()
                .Equals(second.GetValueOrDefault(DateTime.MinValue).ToUniversalTime());
        }
    }
}
