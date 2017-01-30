using System;
using System.Linq;
using HA4IoT.Contracts.Logging;
using HA4IoT.FeatureRebuild.Exceptions;
using HA4IoT.FeatureRebuild.Features;
using HA4IoT.FeatureRebuild.Features.Adapters;

namespace HA4IoT.FeatureRebuild
{
    public static class ComponentExtensions
    {
        public static Component WithAdapter(this Component component, IComponentAdapter componentAdapter)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (componentAdapter == null) throw new ArgumentNullException(nameof(componentAdapter));

            if (component.Adapter != null)
            {
                Log.Warning($"Adapter of component '{component.Id}' is replaced.");
            }

            component.Adapter = componentAdapter;
            return component;
        }

        public static bool SupportsFeature<TFeature>(this Component component) where TFeature : IFeature
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return component.GetFeatures().Any(f => f.GetType() == typeof(TFeature));
        }

        public static void ThrowIfFeatureNotSupported<TFeature>(this Component component) where TFeature : IFeature
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            if (!component.SupportsFeature<TFeature>())
            {
                throw new FeatureNotSupportedException(typeof(TFeature));
            }
        }

        public static TFeature Feature<TFeature>(this Component component) where TFeature : IFeature
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            var feature = component.GetFeatures().OfType<TFeature>().FirstOrDefault();
            if (feature == null)
            {
                throw new FeatureNotSupportedException(typeof(TFeature));
            }

            return feature;
        }
    }
}
