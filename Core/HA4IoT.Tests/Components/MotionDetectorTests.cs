using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Tests.Mockups;
using HA4IoT.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Components
{
    [TestClass]
    public class MotionDetectorTests
    {
        [TestMethod]
        public void MotionDetector_Detect()
        {
            var adapter = new TestMotionDetectorAdapter();
            var motionDetector = CreateMotionDetector(adapter);

            Assert.IsTrue(motionDetector.GetState().Has(MotionDetectionState.Idle));

            adapter.Begin();
            Assert.IsTrue(motionDetector.GetState().Has(MotionDetectionState.MotionDetected));

            adapter.End();
            Assert.IsTrue(motionDetector.GetState().Has(MotionDetectionState.Idle));
        }

        [TestMethod]
        public void MotionDetector_DetectMultiple()
        {
            var adapter = new TestMotionDetectorAdapter();
            var motionDetector = CreateMotionDetector(adapter);

            Assert.IsTrue(motionDetector.GetState().Has(MotionDetectionState.Idle));

            adapter.Begin();
            Assert.IsTrue(motionDetector.GetState().Has(MotionDetectionState.MotionDetected));

            adapter.Begin();
            adapter.Begin();
            adapter.Begin();
            Assert.IsTrue(motionDetector.GetState().Has(MotionDetectionState.MotionDetected));
        }

        private MotionDetector CreateMotionDetector(TestMotionDetectorAdapter adapter)
        {
            var testController = new TestController();
            return new MotionDetector("Test", adapter, testController.GetInstance<ISchedulerService>(), testController.GetInstance<ISettingsService>(), testController.GetInstance<IMessageBrokerService>());
        }
    }
}
