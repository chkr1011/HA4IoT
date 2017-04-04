using HA4IoT.Contracts.Areas;
using HA4IoT.PersonalAgent;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.PersonalAgent
{
    [TestClass]
    public class MessageContextTests
    {
        [TestMethod]
        public void IdentifyAreaIds()
        {
            var testController = new TestController();
            var areaRegistry = testController.GetInstance<IAreaRegistryService>();
            areaRegistry.RegisterArea("Test").Settings.Caption = "Büro";

            var messageContextFactory = testController.GetInstance<MessageContextFactory>();
            var messageContext = messageContextFactory.Create("Hello World. Büro.");
            Assert.AreEqual(1, messageContext.IdentifiedAreaIds.Count);
            Assert.AreEqual(0, messageContext.IdentifiedComponentIds.Count);
            Assert.AreEqual(0, messageContext.IdentifiedCommands.Count);
        }
    }
}
