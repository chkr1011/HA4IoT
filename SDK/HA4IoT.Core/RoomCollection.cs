using HA4IoT.Contracts.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HA4IoT.Core
{
    public class RoomCollection
    { 
        private readonly Dictionary<RoomId, IRoom> _rooms = new Dictionary<RoomId, IRoom>();

        public void Add(IRoom room)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            if (_rooms.ContainsKey(room.Id))
            {
                throw new InvalidOperationException("Room with ID '" + room.Id + "' aready registered.");
            }

            _rooms[room.Id] = room;
        }

        public IRoom this[RoomId id]
        {
            get
            {
                if (id == null) throw new ArgumentNullException(nameof(id));

                IRoom room;
                if (!_rooms.TryGetValue(id, out room))
                {
                    throw new InvalidOperationException("Room with ID '" + id + "' is not registered.");
                }

                return room;
            }
        }

        public IList<IRoom> GetAll()
        {
            return _rooms.Values.ToList();
        }
    }
}
