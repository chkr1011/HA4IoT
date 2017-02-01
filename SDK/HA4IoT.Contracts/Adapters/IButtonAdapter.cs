using System;

namespace HA4IoT.Contracts.Adapters
{
    public interface IButtonAdapter
    {
        event EventHandler Pressed;

        event EventHandler Released;
    }
}
