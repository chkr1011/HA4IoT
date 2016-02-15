using HA4IoT.Contracts.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HA4IoT.Core
{
    public class AreaCollection
    { 
        private readonly Dictionary<AreaId, IArea> _areas = new Dictionary<AreaId, IArea>();

        public void Add(IArea room)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            if (_areas.ContainsKey(room.Id))
            {
                throw new InvalidOperationException("Area with ID '" + room.Id + "' aready registered.");
            }

            _areas[room.Id] = room;
        }

        public IArea this[AreaId id]
        {
            get
            {
                if (id == null) throw new ArgumentNullException(nameof(id));

                IArea room;
                if (!_areas.TryGetValue(id, out room))
                {
                    throw new InvalidOperationException("Room with ID '" + id + "' is not registered.");
                }

                return room;
            }
        }

        public IList<IArea> GetAll()
        {
            return _areas.Values.ToList();
        }
    }
}
