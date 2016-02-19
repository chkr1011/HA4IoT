using System;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Core
{
    public class AreaIdFactory
    {
        public static AreaId CreateIdFrom(Enum value)
        {
            return new AreaId(value.ToString());
        }
    }
}
