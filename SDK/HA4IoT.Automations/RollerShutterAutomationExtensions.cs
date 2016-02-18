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
                area.Controller.Device<IWeatherStation>(),
                area.Controller.HttpApiController,
                area.Controller.Logger);

            area.AddAutomation(automation);

            return automation;
        }
    }
}
