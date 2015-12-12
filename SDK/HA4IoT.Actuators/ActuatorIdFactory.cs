using System;
using System.Collections.Generic;
using HA4IoT.Contracts;

namespace HA4IoT.Actuators
{
    public static class ActuatorIdFactory
    {
        private static readonly HashSet<string> UsedIds = new HashSet<string>();

        public static ActuatorId Create(string id)
        {
            if (!UsedIds.Add(id.ToLowerInvariant()))
            {
                throw new InvalidOperationException("The actuator ID '" + id + "' is already in use.");
            }

            return new ActuatorId(id);
        }
    }
}
