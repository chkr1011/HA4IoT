using System;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Tests.Mockups;
using HA4IoT.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Components
{
    [TestClass]
    public class ButtonTests
    {
        [TestMethod]
        public void Button_FeatureAvailable()
        {
            var testController = new TestController();

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>());

            Assert.IsTrue(button.GetFeatures().Supports<ButtonFeature>());
        }

        [TestMethod]
        public void Button_StateAvailable()
        {
            var testController = new TestController();

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>());

            Assert.IsTrue(button.GetState().Supports<ButtonState>());
        }

        [TestMethod]
        public void Button_PressShortly()
        {
            var testController = new TestController();

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>());

            Assert.IsTrue(button.GetState().Has(ButtonState.Released));
            buttonAdapter.Press();
            Assert.IsTrue(button.GetState().Has(ButtonState.Pressed));
            buttonAdapter.Release();
            Assert.IsTrue(button.GetState().Has(ButtonState.Released));
        }

        [TestMethod]
        public void Button_PressedShortlyTrigger()
        {
            var testController = new TestController();

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>());

            var triggerRaised = false;
            button.PressedShortTrigger.Attach(() => triggerRaised = true);

            Assert.IsTrue(button.GetState().Has(ButtonState.Released));
            buttonAdapter.Touch();
            Assert.IsTrue(triggerRaised);
        }

        [TestMethod]
        public void Button_PressedLongTrigger()
        {
            var testController = new TestController();

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>());

            var triggerRaised = false;
            button.PressedLongTrigger.Attach(() => triggerRaised = true);

            Assert.IsTrue(button.GetState().Has(ButtonState.Released));
            buttonAdapter.Press();

            // Should be false because time was too slow.
            Assert.IsFalse(triggerRaised);

            // 1h is only for test. Enough to test with default settings.
            testController.Tick(TimeSpan.FromHours(1));
            Assert.IsTrue(triggerRaised);
        }

        [TestMethod]
        public void Button_NoDoublePressedLongTrigger()
        {
            var testController = new TestController();

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>());

            var triggerRaisedCount = 0;
            button.PressedLongTrigger.Attach(() => triggerRaisedCount++);

            buttonAdapter.Press();

            // 1h is only for test. Enough to test with default settings.
            testController.Tick(TimeSpan.FromHours(1));
            
            Assert.AreEqual(1, triggerRaisedCount);

            buttonAdapter.Release();

            Assert.AreEqual(1, triggerRaisedCount);
        }
    }
}
