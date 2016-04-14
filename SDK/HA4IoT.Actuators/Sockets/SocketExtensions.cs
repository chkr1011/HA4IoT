using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators.Sockets
{
    public static class SocketExtensions
    {
        public static IArea WithSocket(this IArea room, Enum id, IBinaryOutput output)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (output == null) throw new ArgumentNullException(nameof(output));

            var socket = new Socket(ComponentIdFactory.Create(room, id), new PortBasedBinaryStateEndpoint(output));
            room.AddComponent(socket);

            return room;
        }

        public static ISocket Socket(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.GetComponent<ISocket>(ComponentIdFactory.Create(room, id));
        }
    }
}
