using System;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Contracts.Components
{
    public static class ComponentIdFactory
    {
        public static readonly ComponentId EmptyId = new ComponentId("?");

        public static ComponentId Create(AreaId areaId, Enum id)
        {
            if (areaId == null) throw new ArgumentNullException(nameof(areaId));

            return new ComponentId(areaId + "." + id);
        }
    }
}
