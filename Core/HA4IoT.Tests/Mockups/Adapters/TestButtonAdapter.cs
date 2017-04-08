using System;
using HA4IoT.Contracts.Adapters;

namespace HA4IoT.Tests.Mockups.Adapters
{
    public class TestButtonAdapter : IButtonAdapter
    {
        public event EventHandler Pressed;
        public event EventHandler Released;

        public void Press()
        {
            Pressed?.Invoke(this, EventArgs.Empty);
        }

        public void Release()
        {
            Released?.Invoke(this, EventArgs.Empty);
        }

        public void Touch()
        {
            Press();
            Release();
        }
    }
}
