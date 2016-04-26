using System;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Core
{
    public static class AreaIdFactory
    {
        public static AreaId Create(Enum id)
        {
            return new AreaId(id.ToString());
        }
    }
}
