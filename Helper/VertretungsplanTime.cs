using System;

namespace DominikStiller.VertretungsplanServer.Helper
{
    public class VertretungsplanTime
    {
        static readonly TimeZoneInfo vpTime = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

        public static DateTime Now
        {
            get
            {
                return ConvertUTCToVPTime(DateTime.UtcNow);
            }
        }

        public static DateTime ConvertVPTimeToUTC(DateTime date)
        {
            return TimeZoneInfo.ConvertTime(date, vpTime, TimeZoneInfo.Utc);
        }

        public static DateTime ConvertUTCToVPTime(DateTime date)
        {
            return TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Utc, vpTime);
        }
    }
}
