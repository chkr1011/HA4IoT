using System;

namespace HA4IoT.Automations
{
    public static class RollerShutterAutomationExtensions
    {
        public static RollerShutterAutomation WithDoNotOpenBefore(this RollerShutterAutomation automation, TimeSpan minTime)
        {
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            automation.Settings.SkipBeforeTimestampIsEnabled = true;
            automation.Settings.SkipBeforeTimestamp = minTime;

            return automation;
        }

        public static RollerShutterAutomation WithDoNotOpenIfOutsideTemperatureIsBelowThan(this RollerShutterAutomation automation, float minOutsideTemperature)
        {
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            automation.Settings.SkipIfFrozenIsEnabled = true;
            automation.Settings.SkipIfFrozenTemperature = minOutsideTemperature;

            return automation;
        }

        public static RollerShutterAutomation WithCloseIfOutsideTemperatureIsGreaterThan(this RollerShutterAutomation automation,  float maxOutsideTemperature)
        {
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            automation.Settings.AutoCloseIfTooHotIsEnabled = true;
            automation.Settings.AutoCloseIfTooHotTemperaure = maxOutsideTemperature;

            return automation;
        }
    }
}
