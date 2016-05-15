using FluentAssertions;
using HA4IoT.Contracts.Areas;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.PersonalAgent.Tests
{
    [TestClass]
    public class MessageContextTests
    {
        [TestMethod]
        public void ParseWords()
        {
            var synonymService = new SynonymService();
            var messageContextFactory = new MessageContextFactory(synonymService, new TestController());

            MessageContext messageContext = messageContextFactory.Create(new TestInboundMessage("Hello World. Hello World."));
            messageContext.IdentifiedAreaIds.Count.ShouldBeEquivalentTo(0);
            messageContext.IdentifiedComponentIds.Count.ShouldBeEquivalentTo(0);
            messageContext.IdentifiedComponentStates.Count.ShouldBeEquivalentTo(0);

            messageContext.Words.Count.ShouldBeEquivalentTo(3);
        }

        [TestMethod]
        public void IdentifyAreaIds()
        {
            var synonymService = new SynonymService();
            synonymService.AddSynonymsForArea(new AreaId("Office"), "Büro");

            var messageContextFactory = new MessageContextFactory(synonymService, new TestController());

            MessageContext messageContext = messageContextFactory.Create(new TestInboundMessage("Hello World. Büro."));
            messageContext.IdentifiedAreaIds.Count.ShouldBeEquivalentTo(1);
            messageContext.IdentifiedComponentIds.Count.ShouldBeEquivalentTo(0);
            messageContext.IdentifiedComponentStates.Count.ShouldBeEquivalentTo(0);

            messageContext.Words.Count.ShouldBeEquivalentTo(4);
        }
    }
}
