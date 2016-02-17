using System;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.WeatherStation;

namespace HA4IoT.Automations
{
    public static class AreaExtensions
    {
        public static AutomaticRollerShutterAutomation SetupAutomaticRollerShutters(this IArea area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation = new AutomaticRollerShutterAutomation(
                AutomationIdFactory.CreateIdFrom<AutomaticRollerShutterAutomation>(area),
                area.Controller.Timer, 
                area.Controller.Device<IWeatherStation>(),
                area.Controller.Logger);

            area.AddAutomation(automation);

            return automation;
        }
    }
}
