using System;

namespace HA4IoT.Contracts.Actuators
{
    public interface IButtonEndpoint
    {
        event EventHandler Pressed;

        event EventHandler Released;
    }
}
