using System;
using FluentAssertions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Automations.Tests
{
    [TestClass]
    public class RollerShutterAutomationTests
    {
        private TestController _controller;
        private TestRollerShutter _rollerShutter;
        private TestWeatherStation _weatherStation;
        private TestDaylightService _daylightService;
        private RollerShutterAutomation _automation;

        [TestMethod]
        public void SkipOpen_BecauseTooCold()
        {
            Setup();
            
            _weatherStation.SetTemperature(1.5F);
            _automation.WithDoNotOpenIfOutsideTemperatureIsBelowThan(2);
            _automation.PerformPendingActions();
            _rollerShutter.GetActiveState().ShouldBeEquivalentTo(RollerShutterStateId.Off);

            Setup();

            _weatherStation.SetTemperature(2.5F);
            _automation.WithDoNotOpenIfOutsideTemperatureIsBelowThan(2);
            _automation.PerformPendingActions();
            _rollerShutter.GetActiveState().ShouldBeEquivalentTo(RollerShutterStateId.MovingUp);
        }

        [TestMethod]
        public void Close_BecauseTooHot()
        {
            Setup();
            SkipOpenDueToSunrise();

            _weatherStation.SetTemperature(20F);
            _automation.WithCloseIfOutsideTemperatureIsGreaterThan(25);
            _automation.PerformPendingActions();
            _rollerShutter.GetActiveState().ShouldBeEquivalentTo(RollerShutterStateId.Off);

            _weatherStation.SetTemperature(25.5F);
            _automation.PerformPendingActions();
            _rollerShutter.GetActiveState().ShouldBeEquivalentTo(RollerShutterStateId.MovingDown);
        }

        [TestMethod]
        public void Open_AfterSunrise()
        {
            Setup();

            _rollerShutter.GetActiveState().ShouldBeEquivalentTo(RollerShutterStateId.Off);
            _automation.PerformPendingActions();
            _rollerShutter.GetActiveState().ShouldBeEquivalentTo(RollerShutterStateId.MovingUp);
        }

        [TestMethod]
        public void Close_AfterSunset()
        {
            Setup();
            SkipOpenDueToSunrise();

            _controller.SetTime(TimeSpan.Parse("18:31"));
            
            _automation.PerformPendingActions();
            _rollerShutter.GetActiveState().ShouldBeEquivalentTo(RollerShutterStateId.MovingDown);
        }

        private void SkipOpenDueToSunrise()
        {
            _automation.PerformPendingActions();
            _rollerShutter.GetActiveState().ShouldBeEquivalentTo(RollerShutterStateId.MovingUp);
            _rollerShutter.SetActiveState(RollerShutterStateId.Off);
        }

        private void Setup()
        {
            _controller = new TestController();
            _controller.SetTime(TimeSpan.Parse("12:00"));

            var testRollerShutterFactory = new TestRollerShutterFactory(_controller.Timer);

            _weatherStation = new TestWeatherStation(new DeviceId("Test.WeatherStation"), _controller.Timer);
            _weatherStation.SetTemperature(20);
            _daylightService = new TestDaylightService();

            _rollerShutter = testRollerShutterFactory.CreateTestRollerShutter();
            _controller.RegisterService(_weatherStation);
            _controller.AddComponent(_rollerShutter);

            _automation = new RollerShutterAutomation(
                AutomationIdFactory.EmptyId, 
                _controller.Timer,
                _daylightService,
                _weatherStation,
                _controller);

            _automation.WithRollerShutters(_rollerShutter);
        }
    }
}
