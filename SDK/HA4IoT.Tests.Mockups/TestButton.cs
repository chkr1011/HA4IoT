using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Sensors.Buttons;

namespace HA4IoT.Tests.Mockups
{
    public class TestButton : Button
    {
        public TestButton(ComponentId id, TestButtonEndpoint endpoint, IHomeAutomationTimer timer) 
            : base(id, endpoint, timer)
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
