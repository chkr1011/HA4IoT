using System;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public static class SocketExtensions
    {
        public static IArea WithSocket(this IArea room, Enum id, IBinaryOutput output)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (output == null) throw new ArgumentNullException(nameof(output));

            var socket = new Socket(ActuatorIdFactory.Create(room, id), output, room.Controller.HttpApiController, room.Controller.Logger);
            socket.SetInitialState();

            room.AddActuator(socket);
            return room;
        }

        public static Socket Socket(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.Actuator<Socket>(ActuatorIdFactory.Create(room, id));
        }
    }
}
