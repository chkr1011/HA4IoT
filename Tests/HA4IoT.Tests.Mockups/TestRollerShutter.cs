using System;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Tests.Mockups
{
    public class TestRollerShutter : RollerShutter
    {
        public TestRollerShutter(ComponentId id, TestRollerShutterEndpoint endpoint, IHomeAutomationTimer timer, ISchedulerService schedulerService) 
            : base(id, endpoint, timer, schedulerService)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            Endpoint = endpoint;
        }

        public TestRollerShutterEndpoint Endpoint { get; }
    }
}
