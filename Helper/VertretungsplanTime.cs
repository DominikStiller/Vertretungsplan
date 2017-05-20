using System;

namespace DominikStiller.VertretungsplanServer.Helper
{
    public class VertretungsplanTime
    {
        public static DateTime Now
        {
            get
            {
                return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            }
        }
    }
}
