using FluentAssertions;
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
        public void ParseWords()
        {
            var testController = new TestController();

            var synonymService = new SynonymService(testController.ComponentService);
            var messageContextFactory = new MessageContextFactory(synonymService, testController.AreaService);

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

            var synonymService = new SynonymService(testController.ComponentService);
            synonymService.AddSynonymsForArea(new AreaId("Office"), "Büro");

            var messageContextFactory = new MessageContextFactory(synonymService, testController.AreaService);

            MessageContext messageContext = messageContextFactory.Create(new TestInboundMessage("Hello World. Büro."));
            messageContext.IdentifiedAreaIds.Count.ShouldBeEquivalentTo(1);
            messageContext.IdentifiedComponentIds.Count.ShouldBeEquivalentTo(0);
            messageContext.IdentifiedComponentStates.Count.ShouldBeEquivalentTo(0);

            messageContext.Words.Count.ShouldBeEquivalentTo(3);
        }
    }
}
