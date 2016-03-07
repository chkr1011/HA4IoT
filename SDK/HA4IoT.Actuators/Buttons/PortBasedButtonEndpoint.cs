using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public class ButtonEndpoint : IButtonEndpoint
    {
        private readonly IBinaryInput _input;

        public ButtonEndpoint(IBinaryInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            _input = input;
        }

        public event EventHandler Pressed;
        public event EventHandler Released;
    }
}
