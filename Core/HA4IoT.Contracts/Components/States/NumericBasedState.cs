namespace HA4IoT.Contracts.Components.States
{
    public abstract class NumericBasedState : IComponentFeatureState
    {
        protected NumericBasedState(float? value)
        {
            Value = value;
        }

        public float? Value { get; }
    }
}
