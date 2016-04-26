using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators.Sockets
{
    public static class SocketExtensions
    {
        public static IArea WithSocket(this IArea area, Enum id, IBinaryOutput output)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (output == null) throw new ArgumentNullException(nameof(output));

            var socket = new Socket(ComponentIdFactory.Create(area.Id, id), new PortBasedBinaryStateEndpoint(output));
            area.AddComponent(socket);

            return area;
        }

        public static ISocket Socket(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<ISocket>(ComponentIdFactory.Create(area.Id, id));
        }
    }
}
