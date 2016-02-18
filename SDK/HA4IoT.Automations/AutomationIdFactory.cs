using System;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Configuration;

namespace HA4IoT.Automations
{
    public static class AutomationIdFactory
    {
        public static AutomationId EmptyId { get; } = new AutomationId("?");

        public static AutomationId CreateIdFrom<TAutomation>(IArea area) where TAutomation : IAutomation
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            string value = area.Id.Value + "." + typeof (TAutomation).Name + "-";
            value += area.Automations().Count;

            return new AutomationId(value);
        }
    }
}
