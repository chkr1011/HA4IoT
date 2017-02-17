using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components
{
    public class ComponentFeatureStateCollection
    {
        private readonly HashSet<IComponentFeatureState> _states = new HashSet<IComponentFeatureState>();

        public TComponentFeatureState Extract<TComponentFeatureState>() where TComponentFeatureState : IComponentFeatureState
        {
            var state = _states.FirstOrDefault(s => s is TComponentFeatureState);
            if (state == null)
            {
                throw new ComponentFeatureNotSupportedException(typeof(TComponentFeatureState));
            }

            return (TComponentFeatureState)state;
        }

        public bool Has(IComponentFeatureState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            var foundState = _states.FirstOrDefault(s => s.GetType() == state.GetType());
            if (foundState == null)
            {
                throw new ComponentFeatureNotSupportedException(state.GetType());
            }

            return ReferenceEquals(state, foundState) || foundState.Equals(state);
        }

        public bool Supports<TComponentFeatureState>() where TComponentFeatureState : IComponentFeatureState
        {
            return _states.Any(t => t is TComponentFeatureState);
        }

        public ComponentFeatureStateCollection With(IComponentFeatureState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            if (!_states.Add(state))
            {
                throw new InvalidOperationException();    
            }

            return this;
        }

        public Dictionary<string, JToken> Serialize()
        {
            return _states.ToDictionary(i => i.GetType().Name, i => i.Serialize());
        }
    }
}
