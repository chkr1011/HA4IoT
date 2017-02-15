using System;
using FluentAssertions;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Automations;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Services.Backup;
using HA4IoT.Services.StorageService;
using HA4IoT.Settings;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Automations
{
    [TestClass]
    public class AutomationTests
    {
        [TestMethod]
        public void Automation_Toggle()
        {
            var timer = new TestTimerService();
            var testButtonFactory = new TestButtonFactory(timer, new SettingsService(new BackupService(), new StorageService()));
            
            var testButton = testButtonFactory.CreateTestButton();
            var testOutput = new Lamp(new ComponentId("Lamp1"), new TestBinaryStateAdapter());
            
            CreateAutomation()
                .WithTrigger(testButton.PressedShortlyTrigger)
                .WithActionIfConditionsFulfilled(testOutput.TogglePowerStateAction.Execute);

            Assert.IsTrue(testOutput.GetState().Has(PowerState.Off));
            testButton.PressShortly();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.On));
            testButton.PressShortly();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.Off));
            testButton.PressShortly();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.On));
        }

        [TestMethod]
        public void Automation_WithCondition()
        {
            var testController = new TestController();

            var testButtonFactory = testController.GetInstance<TestButtonFactory>();

            var testButton = testButtonFactory.CreateTestButton();
            var testOutput = new Lamp(new ComponentId("Lamp1"), new TestBinaryStateAdapter());

            new Automation(AutomationIdGenerator.EmptyId)
                .WithTrigger(testButton.PressedShortlyTrigger)
                .WithCondition(ConditionRelation.And, new TimeRangeCondition(testController.GetInstance<IDateTimeService>()).WithStart(TimeSpan.FromHours(1)).WithEnd(TimeSpan.FromHours(2)))
                .WithActionIfConditionsFulfilled(testOutput.TogglePowerStateAction.Execute);

            Assert.IsTrue(testOutput.GetState().Has(PowerState.Off));
            testController.SetTime(TimeSpan.FromHours(0));
            testButton.PressShortly();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.Off));

            testController.SetTime(TimeSpan.FromHours(1.5));
            testButton.PressShortly();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.On));
        }

        private Automation CreateAutomation()
        {
            return new Automation(AutomationIdGenerator.EmptyId);
        }
    }
}
