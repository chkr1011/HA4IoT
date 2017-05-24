using HA4IoT.Actuators.Lamps;
using HA4IoT.Automations;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Environment;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Settings;
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
            var c = new TestController();

            var automation = new ConditionalOnAutomation("Test",
                c.GetInstance<ISchedulerService>(),
                c.GetInstance<IDateTimeService>(),
                c.GetInstance<IDaylightService>());

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, c.GetInstance<ITimerService>(), c.GetInstance<ISettingsService>(), c.GetInstance<IMessageBrokerService>(), c.GetInstance<ILogService>());
            var testOutput = new Lamp("Test", new TestLampAdapter());

            automation.WithTrigger(button.CreatePressedShortTrigger(c.GetInstance<IMessageBrokerService>()));
            automation.WithComponent(testOutput);

            Assert.IsTrue(testOutput.GetState().Has(PowerState.Off));
            buttonAdapter.Touch();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.On));
        }
    }
}
