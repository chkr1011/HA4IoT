using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Sensors.Triggers;
using HA4IoT.Services.Backup;
using HA4IoT.Services.StorageService;
using HA4IoT.Settings;
using HA4IoT.Tests.Mockups;
using HA4IoT.Triggers;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Actuators
{
    [TestClass]
    public class TriggerTests
    {
        [TestMethod]
        public void Trigger_Invoke()
        {
            var attachTriggered = false;
            var eventTriggered = false;

            var trigger = new Trigger();
            trigger.Attach(() => attachTriggered = true);
            Assert.AreEqual(true, trigger.IsAnyAttached);

            trigger.Triggered += (s, e) => eventTriggered = true;
            Assert.AreEqual(true, trigger.IsAnyAttached);

            trigger.Execute();

            Assert.AreEqual(true, attachTriggered);
            Assert.AreEqual(true, eventTriggered);
        }

        [TestMethod]
        public void SensorValueReached_Trigger()
        {
            var testController = new TestController();

            var sensor = new TestTemperatureSensor(
                ComponentIdGenerator.EmptyId,
                testController.GetInstance<ISettingsService>(),
                new TestSensorAdapter());

            var trigger = new SensorValueReachedTrigger(sensor)
            {
                Target = 10.2F,
                Delta = 3.0F
            };

            var triggerCount = 0;
            trigger.Attach(() => triggerCount++);

            sensor.Endpoint.UpdateValue(5);
            Assert.AreEqual(0, triggerCount);

            sensor.Endpoint.UpdateValue(10);
            Assert.AreEqual(0, triggerCount);

            sensor.Endpoint.UpdateValue(10.2F);
            Assert.AreEqual(1, triggerCount);

            sensor.Endpoint.UpdateValue(9.0F);
            Assert.AreEqual(1, triggerCount);

            sensor.Endpoint.UpdateValue(13.0F);
            Assert.AreEqual(1, triggerCount);

            sensor.Endpoint.UpdateValue(5.0F);
            Assert.AreEqual(1, triggerCount);

            sensor.Endpoint.UpdateValue(10.2F);
            Assert.AreEqual(2, triggerCount);
        }

        [TestMethod]
        public void SensorValueUnderran_Trigger()
        {
            var testController = new TestController();

            var sensor = new TestTemperatureSensor(
                ComponentIdGenerator.EmptyId,
                testController.GetInstance<ISettingsService>(),
                new TestSensorAdapter());

            var trigger = new SensorValueUnderranTrigger(sensor)
            {
                Target = 10F,
                Delta = 3F
            };

            var triggerCount = 0;
            trigger.Attach(() => triggerCount++);

            sensor.Endpoint.UpdateValue(5);
            Assert.AreEqual(1, triggerCount);

            sensor.Endpoint.UpdateValue(10);
            Assert.AreEqual(1, triggerCount);

            sensor.Endpoint.UpdateValue(13.1F);
            Assert.AreEqual(1, triggerCount);

            sensor.Endpoint.UpdateValue(9F);
            Assert.AreEqual(2, triggerCount);

            sensor.Endpoint.UpdateValue(13.0F);
            Assert.AreEqual(2, triggerCount);

            sensor.Endpoint.UpdateValue(5F);
            Assert.AreEqual(2, triggerCount);

            sensor.Endpoint.UpdateValue(13.1F);
            Assert.AreEqual(2, triggerCount);

            sensor.Endpoint.UpdateValue(9.9F);
            Assert.AreEqual(3, triggerCount);
        }

        [TestMethod]
        public void Associate_TriggerWithActuatorAction()
        {
            var buttonFactory = new TestButtonFactory(new TestTimerService(), new SettingsService(new BackupService(), new StorageService()));
            var stateMachineFactory = new TestStateMachineFactory();

            var testButton = buttonFactory.CreateTestButton();
            var testOutput = stateMachineFactory.CreateTestStateMachineWithOnOffStates();

            testButton.PressedShortlyTrigger.Attach(testOutput.GetSetNextStateAction());

            testOutput.GetState().Has(PowerState.Off);
            testButton.PressShortly();
            testOutput.GetState().Has(PowerState.On);
            testButton.PressShortly();
            testOutput.GetState().Has(PowerState.Off);
        }
    }
}
