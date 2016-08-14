using System;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Tests.Mockups
{
    public class TestRollerShutter : RollerShutter
    {
        public TestRollerShutter(ComponentId id, TestRollerShutterEndpoint endpoint, ITimerService timerService, ISchedulerService schedulerService) 
            : base(id, endpoint, timerService, schedulerService)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            Endpoint = endpoint;
        }

        public TestRollerShutterEndpoint Endpoint { get; }
    }
}
