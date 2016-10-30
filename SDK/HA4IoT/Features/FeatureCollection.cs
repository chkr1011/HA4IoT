using System;
using System.Collections.Generic;

namespace HA4IoT.Features
{
    public class FeatureCollection
    {
        private readonly Dictionary<Type, IFeature> _features = new Dictionary<Type, IFeature>();

        public void Register(IFeature feature)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));

            var featureType = feature.GetType();

            lock (_features)
            {
                if (_features.ContainsKey(featureType))
                {
                    throw new InvalidOperationException("Feature is already registered.");
                }

                _features[featureType] = feature;
            }
        }

        public TFeature Feature<TFeature>() where TFeature : IFeature
        {
            var feature = SafeFeature<TFeature>();
            if (feature == null)
            {
                throw new NotSupportedException("Feature is not supported.");
            }

            return feature;
        }

        public TFeature SafeFeature<TFeature>() where TFeature : IFeature
        {
            var featureType = typeof(TFeature);

            lock (_features)
            {
                IFeature feature;
                if (!_features.TryGetValue(featureType, out feature))
                {
                    return default(TFeature);
                }

                return (TFeature)feature;
            }
        }

        public bool IsFeatureSupported<TFeature>() where TFeature : IFeature
        {
            lock (_features)
            {
                return _features.ContainsKey(typeof(TFeature));
            }
        }
    }
}
