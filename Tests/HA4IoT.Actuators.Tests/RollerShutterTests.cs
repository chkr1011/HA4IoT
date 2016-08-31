using FluentAssertions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Services.Scheduling;
using HA4IoT.Services.System;
using HA4IoT.Settings;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Actuators.Tests
{
    [TestClass]
    public class RollerShutterTests
    {
        [TestMethod]
        public void TestRollerShutter()
        {
            var timerService = new TestTimerService();
            var rollerShutterFactory = new TestRollerShutterFactory(timerService, new SchedulerService(timerService, new DateTimeService()), new SettingsService());

            TestRollerShutter rollerShutter = rollerShutterFactory.CreateTestRollerShutter();

            rollerShutter.GetState().Equals(RollerShutterStateId.Off).ShouldBeEquivalentTo(true);
            rollerShutter.Endpoint.StopCalledCount.ShouldBeEquivalentTo(1);

            rollerShutter.SetState(RollerShutterStateId.MovingUp);
            rollerShutter.GetState().Equals(RollerShutterStateId.MovingUp).ShouldBeEquivalentTo(true);
            rollerShutter.Endpoint.StartMoveUpCalledCount.ShouldBeEquivalentTo(1);

            rollerShutter.SetState(RollerShutterStateId.MovingDown);
            rollerShutter.GetState().Equals(RollerShutterStateId.MovingDown).ShouldBeEquivalentTo(true);
            rollerShutter.Endpoint.StartMoveDownCalledCount.ShouldBeEquivalentTo(1);

            rollerShutter.SetState(RollerShutterStateId.Off);
            rollerShutter.GetState().Equals(RollerShutterStateId.Off).ShouldBeEquivalentTo(true);
            rollerShutter.Endpoint.StopCalledCount.ShouldBeEquivalentTo(2);

            rollerShutter.Endpoint.StartMoveUpCalledCount.ShouldBeEquivalentTo(1);
            rollerShutter.Endpoint.StartMoveDownCalledCount.ShouldBeEquivalentTo(1);
        }
    }
}
