using System;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Automations;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.Resources;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Tests.Mockups;
using HA4IoT.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Automations
{
    [TestClass]
    public class RollerShutterAutomationTests
    {
        private TestController _controller;
        private RollerShutter _rollerShutter;
        private TestWeatherStation _weatherStation;
        private RollerShutterAutomation _automation;

        [TestMethod]
        public void SkipOpen_BecauseTooCold()
        {
            Setup();

            _weatherStation.OutdoorTemperature = 1.5F;
            _automation.WithDoNotOpenIfOutsideTemperatureIsBelowThan(2);
            _automation.PerformPendingActions();
            _rollerShutter.GetState().Has(PowerState.Off);

            Setup();

            _weatherStation.OutdoorTemperature = 2.5F;
            _automation.WithDoNotOpenIfOutsideTemperatureIsBelowThan(2);
            _automation.PerformPendingActions();
            _rollerShutter.GetState().Has(VerticalMovingState.MovingUp);
            _rollerShutter.GetState().Has(PowerState.Off);
        }

        [TestMethod]
        public void Close_BecauseTooHot()
        {
            Setup();
            SkipOpenDueToSunrise();

            _weatherStation.OutdoorTemperature = 20F;
            _automation.WithCloseIfOutsideTemperatureIsGreaterThan(25);
            _automation.PerformPendingActions();
            _rollerShutter.GetState().Has(PowerState.Off);

            _weatherStation.OutdoorTemperature = 25.5F;
            _automation.PerformPendingActions();
            _rollerShutter.GetState().Has(VerticalMovingState.MovingDown);
            _rollerShutter.GetState().Has(PowerState.On);
        }

        [TestMethod]
        public void Open_AfterSunrise()
        {
            Setup();

            _rollerShutter.GetState().Has(PowerState.Off);
            _automation.PerformPendingActions();
            _rollerShutter.GetState().Has(VerticalMovingState.MovingUp);
            _rollerShutter.GetState().Has(PowerState.On);
        }

        [TestMethod]
        public void Close_AfterSunset()
        {
            Setup();
            SkipOpenDueToSunrise();

            _controller.SetTime(TimeSpan.Parse("18:31"));

            _automation.PerformPendingActions();
            _rollerShutter.GetState().Has(VerticalMovingState.MovingDown);
            _rollerShutter.GetState().Has(PowerState.On);
        }

        private void SkipOpenDueToSunrise()
        {
            _automation.PerformPendingActions();
            _rollerShutter.GetState().Has(VerticalMovingState.MovingUp);
            _rollerShutter.GetState().Has(PowerState.On);
            _rollerShutter.ExecuteCommand(new TurnOffCommand());
        }

        private void Setup()
        {
            _controller = new TestController();
            _controller.SetTime(TimeSpan.Parse("12:00"));

            _weatherStation = new TestWeatherStation { OutdoorTemperature = 20 };

            _rollerShutter = new RollerShutter("Test", new TestRollerShutterAdapter(), _controller.GetInstance<ITimerService>(), _controller.GetInstance<ISettingsService>());
            _controller.GetInstance<IComponentRegistryService>().RegisterComponent(_rollerShutter);

            _automation = new RollerShutterAutomation(
                "Test",
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
