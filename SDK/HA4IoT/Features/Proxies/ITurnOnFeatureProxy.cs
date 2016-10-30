using System;

namespace HA4IoT.Features.Proxies
{
    public interface ITurnOnFeatureProxy
    {
        event EventHandler TurnedOn;

        bool IsTurnedOn { get; }

        void TurnOn();
    }
}
