using System;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Sensors.Buttons
{
    public class EmptyButtonEndpoint : IButtonEndpoint
    {
        public event EventHandler Pressed;
        public event EventHandler Released;
    }
}
