using System;

namespace CK.HomeAutomation.Actuators
{
    public class TimeRangeChecker
    {
        public bool IsTimeInRange(TimeSpan from, TimeSpan until)
        {
            TimeSpan now = DateTime.Now.TimeOfDay;

            if (from < until)
            {
                return now >= from && now <= until;
            }

            return now >= from || now <= until;
        }
    }
}
