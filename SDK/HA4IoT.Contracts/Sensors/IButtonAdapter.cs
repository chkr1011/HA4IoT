using System;

namespace HA4IoT.Contracts.Sensors
{
    public interface IButtonAdapter
    {
        event EventHandler Pressed;

        event EventHandler Released;
    }
}
