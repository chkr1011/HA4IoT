using System;
using HA4IoT.Features.Proxies;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Features
{
    public class TurnOffFeature : IFeature
    {
        private readonly ITurnOffFeatureProxy _proxy;

        public TurnOffFeature(ITurnOffFeatureProxy proxy)
        {
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));

            _proxy = proxy;

            _proxy.TurnedOff += (s, e) => TurnedOff?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler TurnedOff;

        public bool IsTurnedOff => _proxy.IsTurnedOff;

        public void TurnOff()
        {
            _proxy.TurnOff();
        }

        public void Invoke(JToken parameters)
        {
            TurnOff();
        }
    }
}
