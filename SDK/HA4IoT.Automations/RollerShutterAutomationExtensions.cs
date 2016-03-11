using System;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.WeatherStation;

namespace HA4IoT.Automations
{
    public static class RollerShutterAutomationExtensions
    {
        public static RollerShutterAutomation SetupRollerShutterAutomation(this IArea area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation = new RollerShutterAutomation(
                AutomationIdFactory.CreateIdFrom<RollerShutterAutomation>(area),
                area.Controller.Timer, 
                area.Controller.GetDevice<IWeatherStation>(),
                area.Controller.ApiController,
                area.Controller,
                area.Controller.Logger);

            automation.Activate();

            area.AddAutomation(automation);

            return automation;
        }
        
        public static RollerShutterAutomation WithDoNotOpenBefore(this RollerShutterAutomation automation, TimeSpan minTime)
        {
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            automation.Settings.DoNotOpenBeforeIsEnabled.Value = true;
            automation.Settings.DoNotOpenBeforeTime.Value = minTime;

            return automation;
        }

        public static RollerShutterAutomation WithDoNotOpenIfOutsideTemperatureIsBelowThan(this RollerShutterAutomation automation, float minOutsideTemperature)
        {
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            automation.Settings.DoNotOpenIfTooColdIsEnabled.Value = true;
            automation.Settings.DoNotOpenIfTooColdTemperature.Value = minOutsideTemperature;

            return automation;
        }

        public static RollerShutterAutomation WithCloseIfOutsideTemperatureIsGreaterThan(this RollerShutterAutomation automation,  float maxOutsideTemperature)
        {
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            automation.Settings.AutoCloseIfTooHotIsEnabled.Value = true;
            automation.Settings.AutoCloseIfTooHotTemperaure.Value = maxOutsideTemperature;

            return automation;
        }
    }
}
