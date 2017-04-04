using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components
{
    public interface IComponentFeatureStateCollection
    {
        TComponentFeatureState Extract<TComponentFeatureState>() where TComponentFeatureState : IComponentFeatureState;
        bool Has(IComponentFeatureState state);
        Dictionary<string, JToken> Serialize();
        bool Supports<TComponentFeatureState>() where TComponentFeatureState : IComponentFeatureState;
        IComponentFeatureStateCollection With(IComponentFeatureState state);
    }
}