using System;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Core
{
    public static class ControllerExtensions
    {
        public static IRoom CreateRoom(this IController controller, Enum id)
        {
            var roomId = RoomId.From(id);

            // TODO: use RoomIdFactory.
            var room = new Room(roomId, controller);
            controller.AddRoom(room);

            return room;
        }
    }
}
