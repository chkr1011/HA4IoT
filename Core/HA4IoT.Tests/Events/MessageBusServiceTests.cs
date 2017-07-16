using HA4IoT.Contracts.Messaging;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Events
{
    [TestClass]
    public class messageBrokerServiceTests
    {
        [TestMethod]
        public void messageBrokerService_Message_With_Payload()
        {
            var testController = new TestController();
            var bus = testController.GetInstance<IMessageBrokerService>();

            string r = null;
            bus.Subscribe<TestMessageA>("t", m => r = m.Payload.Content.Value);
            bus.Publish("t", new TestMessageA("X")).Wait();

            Assert.AreEqual("X", r);
        }

        [TestMethod]
        public void messageBrokerService_Message_With_Payload_Not_Typed()
        {
            var testController = new TestController();
            var bus = testController.GetInstance<IMessageBrokerService>();

            var r = "";
            bus.Subscribe("t", "TestMessageA", m => r = m.Payload.Content.ToObject<TestMessageA>().Value);
            bus.Publish("t", new TestMessageA("X")).Wait();

            Assert.AreEqual("X", r);
        }

        [TestMethod]
        public void messageBrokerService_Ignore_Different_Sender()
        {
            var testController = new TestController();
            var bus = testController.GetInstance<IMessageBrokerService>();

            string r = null;
            bus.Subscribe<TestMessageA>("t", m => r = m.Payload.Content.Value);
            bus.Publish("wrongT", new TestMessageA("X")).Wait();

            Assert.AreEqual(null, r);
        }

        [TestMethod]
        public void messageBrokerService_Ignore_Different_Message()
        {
            var testController = new TestController();
            var bus = testController.GetInstance<IMessageBrokerService>();

            string r = null;
            bus.Subscribe<TestMessageA>("t", m => r = m.Payload.Content.Value);
            bus.Publish("wrongT", new TestMessageB("X")).Wait();

            Assert.AreEqual(null, r);
        }

        [TestMethod]
        public void messageBrokerService_Filter()
        {
            var testController = new TestController();
            var bus = testController.GetInstance<IMessageBrokerService>();

            string r = null;
            bus.Subscribe<TestMessageA>("t", m => r = m.Payload.Content.Value, m => m.Payload.Content.Value == "Y");
            bus.Publish("t", new TestMessageA("X")).Wait();
            Assert.AreEqual(null, r);
            bus.Publish("t", new TestMessageA("Y")).Wait();
            Assert.AreEqual("Y", r);
        }

        [TestMethod]
        public void messageBrokerService_Subscriber_Check()
        {
            var testController = new TestController();
            var bus = testController.GetInstance<IMessageBrokerService>();

            Assert.IsFalse(bus.HasSubscribers<TestMessageA>("t"));

            bus.Subscribe<TestMessageA>("t", m => {}, m => m.Payload.Content.Value == "Y");

            Assert.IsTrue(bus.HasSubscribers<TestMessageA>("t"));
        }

        [TestMethod]
        public void messageBrokerService_Subscriber_Check_With_Filter()
        {
            var testController = new TestController();
            var bus = testController.GetInstance<IMessageBrokerService>();

            Assert.IsFalse(bus.HasSubscribers<TestMessageA>("t"));

            bus.Subscribe<TestMessageA>("t", m => { }, m => m.Payload.Content.Value == "Y");

            Assert.IsTrue(bus.HasSubscribers<TestMessageA>("t"));
        }

        private class TestMessageA
        {
            public TestMessageA(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        private class TestMessageB
        {
            public TestMessageB(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }
    }
}
