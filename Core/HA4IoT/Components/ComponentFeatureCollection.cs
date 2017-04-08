using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Exceptions;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Components
{
    public class ComponentFeatureCollection : IComponentFeatureCollection
    {
        private readonly HashSet<IComponentFeature> _features = new HashSet<IComponentFeature>();

        public TComponentFeature Extract<TComponentFeature>() where TComponentFeature : IComponentFeature
        {
            var state = _features.FirstOrDefault(s => s is TComponentFeature);
            if (state == null)
            {
                throw new ComponentFeatureNotSupportedException(typeof(TComponentFeature));
            }

            return (TComponentFeature)state;
        }

        public bool Has(IComponentFeature state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            var foundState = _features.FirstOrDefault(s => s.GetType() == state.GetType());
            if (foundState == null)
            {
                throw new ComponentFeatureNotSupportedException(state.GetType());
            }

            return ReferenceEquals(state, foundState) || foundState.Equals(state);
        }

        public bool Supports<TComponentFeature>() where TComponentFeature : IComponentFeature
        {
            return _features.Any(t => t is TComponentFeature);
        }

        public IComponentFeatureCollection With(IComponentFeature feature)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));

            if (!_features.Add(feature))
            {
                throw new InvalidOperationException();
            }

            return this;
        }

        public Dictionary<string, JToken> Serialize()
        {
            return _features.ToDictionary(i => i.GetType().Name, i => i.Serialize());
        }
    }
}
