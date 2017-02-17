using System;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Adapters
{
    public class BinaryInputButtonAdapter : IButtonAdapter
    {
        public BinaryInputButtonAdapter(IBinaryInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            input.StateChanged += DispatchState;
        }

        public event EventHandler Pressed;
        public event EventHandler Released;

        private void DispatchState(object sender, BinaryStateChangedEventArgs e)
        {
            if (e.NewState == BinaryState.High)
            {
                Pressed?.Invoke(this, EventArgs.Empty);
            }
            else if (e.NewState == BinaryState.Low)
            {
                Released?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
