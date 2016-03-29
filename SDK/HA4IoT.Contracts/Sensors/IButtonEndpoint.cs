using System;

namespace HA4IoT.Contracts.Sensors
{
    public interface IButtonEndpoint
    {
        event EventHandler Pressed;

        event EventHandler Released;
    }
}
