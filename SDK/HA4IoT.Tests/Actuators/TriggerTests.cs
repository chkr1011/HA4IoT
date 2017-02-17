using HA4IoT.Actuators.Lamps;
using HA4IoT.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Sensors.Triggers;
using HA4IoT.Tests.Mockups;
using HA4IoT.Tests.Mockups.Adapters;
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
        public void Trigger_SensorValueReached()
        {
            var testController = new TestController();

            var sensor = new TestTemperatureSensor(
                "Test",
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
        public void Trigger_SensorValueUnderran()
        {
            var testController = new TestController();

            var sensor = new TestTemperatureSensor(
                "Test",
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
        public void Trigger_AttachAction()
        {
            var testController = new TestController();
            
            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>());
            var lamp = new Lamp("Test", new TestBinaryOutputAdapter());

            button.PressedShortlyTrigger.Attach(() => lamp.TryTogglePowerState());

            lamp.GetState().Has(PowerState.Off);
            buttonAdapter.Touch();
            lamp.GetState().Has(PowerState.On);
            buttonAdapter.Touch();
            lamp.GetState().Has(PowerState.Off);
        }
    }
}
