using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Actuators.Sockets
{
    public static class SocketExtensions
    {
        public static ISocket GetSocket(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<ISocket>($"{area.Id}.{id}");
        }
    }
}
