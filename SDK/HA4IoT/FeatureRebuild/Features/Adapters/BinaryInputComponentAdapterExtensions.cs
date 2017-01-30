using System;

namespace HA4IoT.FeatureRebuild.Features.Adapters
{
    public static class BinaryInputComponentAdapterExtensions
    {
        public static BinaryInputComponentAdapter WithFeature(this BinaryInputComponentAdapter adapter, IFeature feature)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            if (feature == null) throw new ArgumentNullException(nameof(feature));

            adapter.EnableFeature(feature);
            return adapter;
        }
    }
}
