using System;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Actuators.BinaryStateActuators
{
    public static class BinaryStateActuatorExtensions
    {
        public static BinaryStateActuator GetBinaryStateActuator(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<BinaryStateActuator>($"{area.Id}.{id}");
        }
    }
}
