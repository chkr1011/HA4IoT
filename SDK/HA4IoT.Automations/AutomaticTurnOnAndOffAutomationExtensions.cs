using System;
using HA4IoT.Contracts.Configuration;

namespace HA4IoT.Automations
{
    public static class AutomaticTurnOnAndOffAutomationExtensions
    {
        public static AutomaticTurnOnAndOffAutomation SetupAutomaticTurnOnAndOffAutomation(this IArea area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation =
                new AutomaticTurnOnAndOffAutomation(
                    AutomationIdFactory.CreateIdFrom<AutomaticTurnOnAndOffAutomation>(area), area.Controller.Timer);

            area.AddAutomation(automation);

            return automation;
        }
    }
}
