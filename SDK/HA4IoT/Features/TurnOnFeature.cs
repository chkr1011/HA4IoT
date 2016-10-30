using System;
using HA4IoT.Features.Proxies;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Features
{
    public class TurnOnFeature : IFeature
    {
        private readonly ITurnOnFeatureProxy _proxy;

        public TurnOnFeature(ITurnOnFeatureProxy proxy)
        {
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));

            _proxy = proxy;

            _proxy.TurnedOn += (s, e) => TurnedOn?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler TurnedOn;

        public bool IsTurnedOn => _proxy.IsTurnedOn;

        public void TurnOn()
        {
            _proxy.TurnOn();
        }

        public void Invoke(JToken parameters)
        {
            TurnOn();
        }
    }
}
