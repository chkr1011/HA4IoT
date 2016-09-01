using System;

namespace HA4IoT.Contracts.Areas
{
    public static class AreaIdFactory
    {
        public static AreaId Create(Enum id)
        {
            return new AreaId(id.ToString());
        }
    }
}
