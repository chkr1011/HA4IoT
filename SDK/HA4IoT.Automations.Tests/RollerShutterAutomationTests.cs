using HA4IoT.Actuators;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Automations.Tests
{
    [TestClass]
    public class RollerShutterAutomationTests
    {
        [TestMethod]
        public void CloseIfTooCold()
        {
            var testTimer = new TestHomeAutomationTimer();
            //var rollerShutterAutomation = new RollerShutterAutomation(ActuatorIdFactory.EmptyId, testTimer, );
        }
    }
}
