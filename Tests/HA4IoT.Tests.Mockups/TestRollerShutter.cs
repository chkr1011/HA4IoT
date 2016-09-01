using System;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Tests.Mockups
{
    public class TestRollerShutter : RollerShutter
    {
        public TestRollerShutter(ComponentId id, TestRollerShutterEndpoint endpoint, ITimerService timerService, ISchedulerService schedulerService, ISettingsService settingsService) 
            : base(id, endpoint, timerService, schedulerService, settingsService)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            Endpoint = endpoint;
        }

        public TestRollerShutterEndpoint Endpoint { get; }
    }
}
