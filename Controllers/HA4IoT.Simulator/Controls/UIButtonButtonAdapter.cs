using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using HA4IoT.Contracts.Adapters;

namespace HA4IoT.Simulator.Controls
{
    public class UIButtonAdapter : IButtonAdapter
    {
        private readonly Button _button;

        public UIButtonAdapter(Button button)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));

            _button = button;
        }

        public event EventHandler Pressed;
        public event EventHandler Released;

        public void Connect()
        {
            _button.Click += (s, e) => PressAndRelease();
            _button.PointerPressed += (s, e) => Press();
            _button.PointerReleased += (s, e) => Release();
        }

        private void Press()
        {
            Task.Run(() => Pressed?.Invoke(this, EventArgs.Empty));
        }

        private void Release()
        {
            Task.Run(() => Released?.Invoke(this, EventArgs.Empty));
        }

        private void PressAndRelease()
        {
            Task.Run(() =>
            {
                Pressed?.Invoke(this, EventArgs.Empty);
                Released?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}
