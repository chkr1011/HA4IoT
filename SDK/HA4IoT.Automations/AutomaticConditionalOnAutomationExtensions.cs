using System;
using HA4IoT.Contracts.Configuration;

namespace HA4IoT.Automations
{
    public static class AutomaticConditionalOnAutomationExtensions
    {
        public static AutomaticConditionalOnAutomation SetupAutomaticConditionalOnAutomation(this IArea area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation =
                new AutomaticConditionalOnAutomation(
                    AutomationIdFactory.CreateIdFrom<AutomaticConditionalOnAutomation>(area), area.Controller.Timer);

            area.AddAutomation(automation);

            return automation;
        }
    }
}
