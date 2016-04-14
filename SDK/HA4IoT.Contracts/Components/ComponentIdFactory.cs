using System;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Contracts.Components
{
    public static class ComponentIdFactory
    {
        public static readonly ComponentId EmptyId = new ComponentId("?");

        public static ComponentId Create(IArea room, Enum id)
        {
            return new ComponentId(room.Id + "." + id);
        }
    }
}
