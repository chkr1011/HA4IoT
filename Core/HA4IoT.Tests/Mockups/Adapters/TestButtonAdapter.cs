using System;
using HA4IoT.Contracts.Adapters;

namespace HA4IoT.Tests.Mockups.Adapters
{
    public class TestButtonAdapter : IButtonAdapter
    {
        public event EventHandler<ButtonAdapterStateChangedEventArgs> StateChanged;

        public void Press()
        {
            StateChanged?.Invoke(this, new ButtonAdapterStateChangedEventArgs(AdapterButtonState.Pressed));
        }

        public void Release()
        {
            StateChanged?.Invoke(this, new ButtonAdapterStateChangedEventArgs(AdapterButtonState.Released));
        }

        public void Touch()
        {
            Press();
            Release();
        }
    }
}
