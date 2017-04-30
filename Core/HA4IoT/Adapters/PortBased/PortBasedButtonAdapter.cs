using System;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Adapters.PortBased
{
    public class PortBasedButtonAdapter : IButtonAdapter
    {
        public PortBasedButtonAdapter(IBinaryInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            input.StateChanged += ForwardState;
        }

        public event EventHandler<ButtonAdapterStateChangedEventArgs> StateChanged;
        
        private void ForwardState(object sender, BinaryStateChangedEventArgs e)
        {
            if (e.NewState == BinaryState.High)
            {
                StateChanged?.Invoke(this, new ButtonAdapterStateChangedEventArgs(AdapterButtonState.Pressed));
            }
            else if (e.NewState == BinaryState.Low)
            {
                StateChanged?.Invoke(this, new ButtonAdapterStateChangedEventArgs(AdapterButtonState.Released));
            }
        }
    }
}
