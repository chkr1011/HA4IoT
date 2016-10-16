using FluentAssertions;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Sensors.Triggers;
using HA4IoT.Services.Backup;
using HA4IoT.Services.StorageService;
using HA4IoT.Settings;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Actuators
{
    [TestClass]
    public class TriggerTests
    {
        [TestMethod]
        public void Trigger_Invoke()
        {
            bool attachTriggered = false;
            bool eventTriggered = false;

            var trigger = new Trigger();
            trigger.Attach(() => attachTriggered = true);
            trigger.IsAnyAttached.ShouldBeEquivalentTo(true);

            trigger.Triggered += (s, e) => eventTriggered = true;
            trigger.IsAnyAttached.ShouldBeEquivalentTo(true);

            trigger.Execute();

            attachTriggered.ShouldBeEquivalentTo(true);
            eventTriggered.ShouldBeEquivalentTo(true);
        }

        [TestMethod]
        public void SensorValueReached_Trigger()
        {
            var sensor = new TestTemperatureSensor(ComponentIdGenerator.EmptyId, new SettingsService(new BackupService(), new StorageService()), new TestNumericValueSensorEndpoint());
            var trigger = new SensorValueReachedTrigger(sensor);
            trigger.Target = 10.2F;
            trigger.Delta = 3.0F;

            int triggerCount = 0;
            trigger.Attach(() => triggerCount++);

            sensor.Endpoint.UpdateValue(5);
            triggerCount.ShouldBeEquivalentTo(0);

            sensor.Endpoint.UpdateValue(10);
            triggerCount.ShouldBeEquivalentTo(0);

            sensor.Endpoint.UpdateValue(10.2F);
            triggerCount.ShouldBeEquivalentTo(1);

            sensor.Endpoint.UpdateValue(9.0F);
            triggerCount.ShouldBeEquivalentTo(1);

            sensor.Endpoint.UpdateValue(13.0F);
            triggerCount.ShouldBeEquivalentTo(1);

            sensor.Endpoint.UpdateValue(5.0F);
            triggerCount.ShouldBeEquivalentTo(1);

            sensor.Endpoint.UpdateValue(10.2F);
            triggerCount.ShouldBeEquivalentTo(2);
        }

        [TestMethod]
        public void SensorValueUnderran_Trigger()
        {
            var sensor = new TestTemperatureSensor(ComponentIdGenerator.EmptyId, new SettingsService(new BackupService(), new StorageService()), new TestNumericValueSensorEndpoint());
            var trigger = new SensorValueUnderranTrigger(sensor);
            trigger.Target = 10F;
            trigger.Delta = 3F;

            int triggerCount = 0;
            trigger.Attach(() => triggerCount++);

            sensor.Endpoint.UpdateValue(5);
            triggerCount.ShouldBeEquivalentTo(1);

            sensor.Endpoint.UpdateValue(10);
            triggerCount.ShouldBeEquivalentTo(1);

            sensor.Endpoint.UpdateValue(13.1F);
            triggerCount.ShouldBeEquivalentTo(1);

            sensor.Endpoint.UpdateValue(9F);
            triggerCount.ShouldBeEquivalentTo(2);

            sensor.Endpoint.UpdateValue(13.0F);
            triggerCount.ShouldBeEquivalentTo(2);

            sensor.Endpoint.UpdateValue(5F);
            triggerCount.ShouldBeEquivalentTo(2);

            sensor.Endpoint.UpdateValue(13.1F);
            triggerCount.ShouldBeEquivalentTo(2);

            sensor.Endpoint.UpdateValue(9.9F);
            triggerCount.ShouldBeEquivalentTo(3);
        }

        [TestMethod]
        public void Associate_TriggerWithActuatorAction()
        {
            var buttonFactory = new TestButtonFactory(new TestTimerService(), new SettingsService(new BackupService(), new StorageService()));
            var stateMachineFactory = new TestStateMachineFactory();

            var testButton = buttonFactory.CreateTestButton();
            var testOutput = stateMachineFactory.CreateTestStateMachineWithOnOffStates();

            testButton.GetPressedShortlyTrigger().Attach(testOutput.GetSetNextStateAction());

            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);
            testButton.PressShortly();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.On);
            testButton.PressShortly();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryStateId.Off);
        }
    }
}
