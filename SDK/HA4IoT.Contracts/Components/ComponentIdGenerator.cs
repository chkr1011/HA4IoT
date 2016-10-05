using System;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Contracts.Components
{
    public static class ComponentIdGenerator
    {
        public static readonly ComponentId EmptyId = new ComponentId("?");

        public static ComponentId Generate(AreaId areaId, Enum id)
        {
            if (areaId == null) throw new ArgumentNullException(nameof(areaId));

            return new ComponentId(areaId + "." + id);
        }

        public static ComponentId Generate(IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return new ComponentId(area.Id + "." + id);
        }
    }
}
