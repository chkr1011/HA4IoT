using System;

namespace HA4IoT.Automations
{
    public static class RollerShutterAutomationExtensions
    {
        public static RollerShutterAutomation WithDoNotOpenBefore(this RollerShutterAutomation automation, TimeSpan minTime)
        {
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            automation.SpecialSettingsWrapper.SkipBeforeTimestampIsEnabled = true;
            automation.SpecialSettingsWrapper.SkipBeforeTimestamp = minTime;

            return automation;
        }

        public static RollerShutterAutomation WithDoNotOpenIfOutsideTemperatureIsBelowThan(this RollerShutterAutomation automation, float minOutsideTemperature)
        {
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            automation.SpecialSettingsWrapper.SkipIfFrozenIsEnabled = true;
            automation.SpecialSettingsWrapper.SkipIfFrozenTemperature = minOutsideTemperature;

            return automation;
        }

        public static RollerShutterAutomation WithCloseIfOutsideTemperatureIsGreaterThan(this RollerShutterAutomation automation,  float maxOutsideTemperature)
        {
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            automation.SpecialSettingsWrapper.AutoCloseIfTooHotIsEnabled = true;
            automation.SpecialSettingsWrapper.AutoCloseIfTooHotTemperaure = maxOutsideTemperature;

            return automation;
        }
    }
}
