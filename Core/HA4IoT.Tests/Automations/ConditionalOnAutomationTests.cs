using HA4IoT.Actuators.Lamps;
using HA4IoT.Automations;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Tests.Mockups;
using HA4IoT.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Automations
{
    [TestClass]
    public class ConditionalOnAutomationTests
    {
        [TestMethod]
        public void Empty_ConditionalOnAutomation()
        {
            var testController = new TestController();

            var automation = new ConditionalOnAutomation("Test",
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<IDateTimeService>(),
                testController.GetInstance<IDaylightService>());

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>());
            var testOutput = new Lamp("Test", new TestLampAdapter());

            automation.WithTrigger(button.PressedShortTrigger);
            automation.WithComponent(testOutput);

            Assert.IsTrue(testOutput.GetState().Has(PowerState.Off));
            buttonAdapter.Touch();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.On));
        }
    }
}
