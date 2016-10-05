using System;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Contracts.Automations
{
    public static class AutomationIdGenerator
    {
        public static readonly AutomationId EmptyId = new AutomationId("?");

        public static AutomationId Generate(AreaId areaId, Enum id)
        {
            if (areaId == null) throw new ArgumentNullException(nameof(areaId));

            return new AutomationId(areaId + "." + id);
        }

        public static AutomationId Generate(IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return new AutomationId(area.Id + "." + id);
        }
    }
}
