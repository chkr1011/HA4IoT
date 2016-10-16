using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Sensors.Buttons;

namespace HA4IoT.Tests.Mockups
{
    public class TestButton : Button
    {
        public TestButton(ComponentId id, TestButtonEndpoint endpoint, ITimerService timerService, ISettingsService settingsService) 
            : base(id, endpoint, timerService, settingsService)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            Endpoint = endpoint;
        }

        public TestButtonEndpoint Endpoint { get; }

        public void PressShortly()
        {
            OnPressedShortlyShort();
        }

        public void PressLong()
        {
            OnPressedLong();
        }
    }
}
