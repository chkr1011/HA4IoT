using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Controller.Local
{
    public class UIButtonButtonEndpoint : IButtonEndpoint
    {
        public UIButtonButtonEndpoint(Button button)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));

            button.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    button.PointerPressed += (s, e) => Pressed?.Invoke(this, EventArgs.Empty);
                    button.PointerReleased += (s, e) => Released?.Invoke(this, EventArgs.Empty);
                }).AsTask().Wait();
        }

        public event EventHandler Pressed;
        public event EventHandler Released;
    }
}
