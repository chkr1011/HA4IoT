using System.Collections.Generic;

namespace HA4IoT.FeatureRebuild.Features
{
    public class SetStateFeature : IFeature
    {
        public List<string> SupportedStates { get; set; } = new List<string>();
    }
}
