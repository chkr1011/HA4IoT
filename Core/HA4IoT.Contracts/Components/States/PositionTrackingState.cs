namespace HA4IoT.Contracts.Components.States
{
    public class PositionTrackingState : IComponentFeatureState
    {
        public PositionTrackingState(int position, bool isClosed)
        {
            Position = position;
            IsClosed = isClosed;
        }

        public int Position { get; }

        public bool IsClosed { get; }
    }
}
