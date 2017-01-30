using System.Collections.Generic;
using HA4IoT.Components;
using HA4IoT.FeatureRebuild.Features;

namespace HA4IoT.FeatureRebuild.Api
{
    public class ComponentConfiguration
    {
        public ComponentSettings Settings { get; set; }

        public Dictionary<string, IFeature> Features { get; set; }
    }
}
