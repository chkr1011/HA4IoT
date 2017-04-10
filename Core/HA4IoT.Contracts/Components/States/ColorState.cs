namespace HA4IoT.Contracts.Components.States
{
    public class ColorState : IComponentFeatureState
    {
        public double Hue { get; set; }

        public double Saturation { get; set; }

        public double Value { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as ColorState;
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(other, this))
            {
                return true;
            }

            return Hue.Equals(other.Hue) && Saturation.Equals(other.Saturation) && Value.Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return Hue.GetHashCode() ^ Saturation.GetHashCode() ^ Value.GetHashCode();
        }
    }
}
