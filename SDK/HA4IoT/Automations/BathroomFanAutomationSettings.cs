using System;

namespace HA4IoT.Automations
{
    public class BathroomFanAutomationSettings : AutomationSettings
    {
        private TimeSpan _slowDuration = TimeSpan.FromMinutes(8);
        private TimeSpan _fastDuration = TimeSpan.FromMinutes(12);

        public TimeSpan SlowDuration
        {
            get { return _slowDuration; }
            set
            {
                _slowDuration = value;
                OnValueChanged(nameof(SlowDuration));
            }
        }

        public TimeSpan FastDuration
        {
            get { return _fastDuration; }
            set
            {
                _fastDuration = value;
                OnValueChanged(nameof(FastDuration));
            }
        }
    }
}
