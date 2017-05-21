using HA4IoT.Actuators.Lamps;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Sensors.Buttons;
using HA4IoT.Tests.Mockups;
using HA4IoT.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Services
{
    [TestClass]
    public class ComponentRegistryServiceTests
    {
        [TestMethod]
        public void ComponentRegistryService_GetGeneric()
        {
            var c = new TestController();
            var s = c.GetInstance<IComponentRegistryService>();

            var l1 = new Lamp("L1", new TestLampAdapter());
            s.RegisterComponent(l1);

            var l2 = new Lamp("L2", new TestLampAdapter());
            s.RegisterComponent(l2);

            Assert.AreEqual(2, s.GetComponents<ILamp>().Count);
        }

        [TestMethod]
        public void ComponentRegistryService_GetGenericMatchesOnly()
        {
            var c = new TestController();
            var s = c.GetInstance<IComponentRegistryService>();

            var l1 = new Lamp("L1", new TestLampAdapter());
            s.RegisterComponent(l1);

            var l2 = new Lamp("L2", new TestLampAdapter());
            s.RegisterComponent(l2);

            var b1 = new Button("B1", new TestButtonAdapter(), c.GetInstance<ITimerService>(), c.GetInstance<ISettingsService>(), c.GetInstance<IMessageBrokerService>(), c.GetInstance<ILogService>());
            s.RegisterComponent(b1);

            Assert.AreEqual(1, s.GetComponents<IButton>().Count);
        }
    }
}
