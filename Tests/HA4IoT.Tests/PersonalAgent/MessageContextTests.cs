using FluentAssertions;
using HA4IoT.PersonalAgent;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.PersonalAgent
{
    [TestClass]
    public class MessageContextTests
    {
        [TestMethod]
        public void ParseWords()
        {
            var testController = new TestController();

            var messageContextFactory = new MessageContextFactory(testController.AreaService, testController.ComponentService, testController.SettingsService);

            MessageContext messageContext = messageContextFactory.Create(new TestInboundMessage("Hello World. Hello World."));
            messageContext.IdentifiedAreaIds.Count.ShouldBeEquivalentTo(0);
            messageContext.IdentifiedComponentIds.Count.ShouldBeEquivalentTo(0);
            messageContext.IdentifiedComponentStates.Count.ShouldBeEquivalentTo(0);

            messageContext.Words.Count.ShouldBeEquivalentTo(2);
        }

        [TestMethod]
        public void IdentifyAreaIds()
        {
            var testController = new TestController();
            
            var messageContextFactory = new MessageContextFactory(testController.AreaService, testController.ComponentService, testController.SettingsService);

            MessageContext messageContext = messageContextFactory.Create(new TestInboundMessage("Hello World. Büro."));
            messageContext.IdentifiedAreaIds.Count.ShouldBeEquivalentTo(1);
            messageContext.IdentifiedComponentIds.Count.ShouldBeEquivalentTo(0);
            messageContext.IdentifiedComponentStates.Count.ShouldBeEquivalentTo(0);

            messageContext.Words.Count.ShouldBeEquivalentTo(3);
        }
    }
}
