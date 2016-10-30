using System;

namespace HA4IoT.Features.Proxies
{
    public interface ITurnOffFeatureProxy
    {
        event EventHandler TurnedOff;

        bool IsTurnedOff { get; }

        void TurnOff();
    }
}
