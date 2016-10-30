namespace HA4IoT.Components
{
    public class ComponentSettings
    {
        public bool IsEnabled { get; set; }

        public string Caption { get; set; }

        public ComponentClass Class { get; set; } = ComponentClass.Other;
    }
}
