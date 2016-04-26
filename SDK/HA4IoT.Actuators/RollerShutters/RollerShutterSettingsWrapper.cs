using System;
using HA4IoT.Components;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Actuators.RollerShutters
{
    public class RollerShutterSettingsWrapper : ComponentSettingsWrapper
    {
        private const string MaxPositionName = "MaxPosition";
        private const string AutoOffTimeoutName = "AutoOffTimeout";

        public RollerShutterSettingsWrapper(ISettingsContainer settings) 
            : base(settings)
        {
            MaxPosition = 20000;
            AutoOffTimeout = TimeSpan.FromSeconds(22);
        }

        public int MaxPosition
        {
            get { return Settings.GetInteger(MaxPositionName); }
            set { Settings.SetValue(MaxPositionName, value); }
        }

        public TimeSpan AutoOffTimeout
        {
            get { return Settings.GetTimeSpan(AutoOffTimeoutName); }
            set { Settings.SetValue(AutoOffTimeoutName, value); }
        }
    }
}
