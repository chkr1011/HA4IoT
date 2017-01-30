using HA4IoT.FeatureRebuild;

namespace HA4IoT.Components
{
    public class ComponentSettings
    {
        public bool IsEnabled { get; set; } = true;

        public bool IsVisible { get; set; } = true;

        public string Caption { get; set; }
        
        public ComponentClass Class { get; set; } = ComponentClass.Other;
    }
}
