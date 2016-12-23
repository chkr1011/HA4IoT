namespace HA4IoT.Contracts.Components
{
    public static class ComponentIdExtensions
    {
        public static ComponentId AsComponentId(this string value)
        {
            return new ComponentId(value);
        }
    }
}
