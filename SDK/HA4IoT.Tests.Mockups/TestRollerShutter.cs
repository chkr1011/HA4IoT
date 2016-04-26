using System;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Tests.Mockups
{
    public class TestRollerShutter : RollerShutter
    {
        public TestRollerShutter(ComponentId id, TestRollerShutterEndpoint endpoint, IHomeAutomationTimer timer) 
            : base(id, endpoint, timer)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            Endpoint = endpoint;
        }

        public TestRollerShutterEndpoint Endpoint { get; }
    }
}
