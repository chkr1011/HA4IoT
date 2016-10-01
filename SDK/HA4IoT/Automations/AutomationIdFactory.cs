using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;

namespace HA4IoT.Automations
{
    public static class AutomationIdFactory
    {
        public static AutomationId EmptyId { get; } = new AutomationId("?");

        public static AutomationId CreateIdFrom<TAutomation>(IArea area) where TAutomation : IAutomation
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            string value = area.Id.Value + "." + typeof (TAutomation).Name + "-";
            value += area.GetAutomations().Count;

            return new AutomationId(value);
        }
    }
}
