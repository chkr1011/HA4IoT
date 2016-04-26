using System;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Automations
{
    public static class TurnOnAndOffAutomationExtensions
    {
        public static TurnOnAndOffAutomation SetupTurnOnAndOffAutomation(this IArea area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation =
                new TurnOnAndOffAutomation(
                    AutomationIdFactory.CreateIdFrom<TurnOnAndOffAutomation>(area),
                    area.Controller.Timer);

            area.AddAutomation(automation);

            return automation;
        }
    }
}
