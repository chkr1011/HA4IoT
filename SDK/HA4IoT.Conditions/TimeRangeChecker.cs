using System;

namespace HA4IoT.Conditions
{
    public class TimeRangeChecker
    {
        public bool IsTimeInRange(TimeSpan time, TimeSpan from, TimeSpan until)
        {
            if (from < until)
            {
                return time >= from && time <= until;
            }

            return time >= from || time <= until;
        }
    }
}
