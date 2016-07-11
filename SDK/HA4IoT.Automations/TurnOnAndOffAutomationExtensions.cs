using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

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
                    area.Controller.ServiceLocator.GetService<IDateTimeService>(),
                    area.Controller.ServiceLocator.GetService<ISchedulerService>());

            area.AddAutomation(automation);

            return automation;
        }
    }
}
