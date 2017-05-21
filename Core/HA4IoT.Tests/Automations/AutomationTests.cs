using System;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Automations;
using HA4IoT.Components;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Tests.Mockups;
using HA4IoT.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Automations
{
    [TestClass]
    public class AutomationTests
    {
        [TestMethod]
        public void Automation_Toggle()
        {
            var c = new TestController();

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, c.GetInstance<ITimerService>(), c.GetInstance<ISettingsService>(), c.GetInstance<IMessageBrokerService>(), c.GetInstance<ILogService>());
            var testOutput = new Lamp("Test", new TestLampAdapter());
            
            new Automation("Test")
                .WithTrigger(button.CreatePressedShortTrigger(c.GetInstance<IMessageBrokerService>()))
                .WithActionIfConditionsFulfilled(() => testOutput.TryTogglePowerState());

            Assert.IsTrue(testOutput.GetState().Has(PowerState.Off));
            buttonAdapter.Touch();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.On));
            buttonAdapter.Touch();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.Off));
            buttonAdapter.Touch();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.On));
        }

        [TestMethod]
        public void Automation_WithCondition()
        {
            var c = new TestController();

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, c.GetInstance<ITimerService>(), c.GetInstance<ISettingsService>(), c.GetInstance<IMessageBrokerService>(), c.GetInstance<ILogService>());
            var testOutput = new Lamp("Test", new TestLampAdapter());

            new Automation("Test")
                .WithTrigger(button.CreatePressedShortTrigger(c.GetInstance<IMessageBrokerService>()))
                .WithCondition(ConditionRelation.And, new TimeRangeCondition(c.GetInstance<IDateTimeService>()).WithStart(TimeSpan.FromHours(1)).WithEnd(TimeSpan.FromHours(2)))
                .WithActionIfConditionsFulfilled(() => testOutput.TryTogglePowerState());

            Assert.IsTrue(testOutput.GetState().Has(PowerState.Off));
            c.SetTime(TimeSpan.FromHours(0));
            buttonAdapter.Touch();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.Off));

            c.SetTime(TimeSpan.FromHours(1.5));
            buttonAdapter.Touch();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.On));
        }
    }
}
