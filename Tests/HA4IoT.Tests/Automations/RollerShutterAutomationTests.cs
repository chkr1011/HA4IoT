using System;
using FluentAssertions;
using HA4IoT.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.Resources;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Automations
{
    [TestClass]
    public class RollerShutterAutomationTests
    {
        private TestController _controller;
        private TestRollerShutter _rollerShutter;
        private TestWeatherStation _weatherStation;
        private RollerShutterAutomation _automation;

        [TestMethod]
        public void SkipOpen_BecauseTooCold()
        {
            Setup();

            _weatherStation.OutdoorTemperature = 1.5F;
            _automation.WithDoNotOpenIfOutsideTemperatureIsBelowThan(2);
            _automation.PerformPendingActions();
            _rollerShutter.GetState().ShouldBeEquivalentTo(RollerShutterStateId.Off);

            Setup();

            _weatherStation.OutdoorTemperature = 2.5F;
            _automation.WithDoNotOpenIfOutsideTemperatureIsBelowThan(2);
            _automation.PerformPendingActions();
            _rollerShutter.GetState().ShouldBeEquivalentTo(RollerShutterStateId.MovingUp);
        }

        [TestMethod]
        public void Close_BecauseTooHot()
        {
            Setup();
            SkipOpenDueToSunrise();

            _weatherStation.OutdoorTemperature = 20F;
            _automation.WithCloseIfOutsideTemperatureIsGreaterThan(25);
            _automation.PerformPendingActions();
            _rollerShutter.GetState().ShouldBeEquivalentTo(RollerShutterStateId.Off);

            _weatherStation.OutdoorTemperature = 25.5F;
            _automation.PerformPendingActions();
            _rollerShutter.GetState().ShouldBeEquivalentTo(RollerShutterStateId.MovingDown);
        }

        [TestMethod]
        public void Open_AfterSunrise()
        {
            Setup();

            _rollerShutter.GetState().ShouldBeEquivalentTo(RollerShutterStateId.Off);
            _automation.PerformPendingActions();
            _rollerShutter.GetState().ShouldBeEquivalentTo(RollerShutterStateId.MovingUp);
        }

        [TestMethod]
        public void Close_AfterSunset()
        {
            Setup();
            SkipOpenDueToSunrise();

            _controller.SetTime(TimeSpan.Parse("18:31"));

            _automation.PerformPendingActions();
            _rollerShutter.GetState().ShouldBeEquivalentTo(RollerShutterStateId.MovingDown);
        }

        private void SkipOpenDueToSunrise()
        {
            _automation.PerformPendingActions();
            _rollerShutter.GetState().ShouldBeEquivalentTo(RollerShutterStateId.MovingUp);
            _rollerShutter.ChangeState(RollerShutterStateId.Off);
        }

        private void Setup()
        {
            _controller = new TestController();
            _controller.SetTime(TimeSpan.Parse("12:00"));

            var testRollerShutterFactory = _controller.GetInstance<TestRollerShutterFactory>();

            _weatherStation = new TestWeatherStation { OutdoorTemperature = 20 };

            _rollerShutter = testRollerShutterFactory.CreateTestRollerShutter();
            _controller.GetInstance<IComponentRegistryService>().AddComponent(_rollerShutter);

            _automation = new RollerShutterAutomation(
                AutomationIdGenerator.EmptyId,
                _controller.GetInstance<INotificationService>(),
                _controller.GetInstance<ISchedulerService>(),
                _controller.GetInstance<IDateTimeService>(),
                _controller.GetInstance<IDaylightService>(),
                _weatherStation,
                _controller.GetInstance<IComponentRegistryService>(),
                _controller.GetInstance<ISettingsService>(),
                _controller.GetInstance<IResourceService>());

            _automation.WithRollerShutters(_rollerShutter);
        }
    }
}
