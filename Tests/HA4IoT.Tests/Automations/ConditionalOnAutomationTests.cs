using FluentAssertions;
using HA4IoT.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Services.Backup;
using HA4IoT.Services.StorageService;
using HA4IoT.Settings;
using HA4IoT.Tests.Mockups;
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
            var automation = new ConditionalOnAutomation(AutomationIdGenerator.EmptyId,
                testController.SchedulerService,
                testController.DateTimeService,
                testController.DaylightService);

            var testButtonFactory = new TestButtonFactory(testController.TimerService, new SettingsService(new BackupService(), new StorageService()));
            var testStateMachineFactory = new TestStateMachineFactory();

            var testButton = testButtonFactory.CreateTestButton();
            var testOutput = testStateMachineFactory.CreateTestStateMachineWithOnOffStates();

            automation.WithTrigger(testButton.GetPressedShortlyTrigger());
            automation.WithActuator(testOutput);
            
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);
            testButton.PressShortly();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.On);
        }
    }
}
