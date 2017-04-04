using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Mqtt
{
    [TestClass]
    public class MqttTests
    {
        [TestMethod]
        public void Mqtt_SingleByte()
        {
            ////var c1 = new MqttInMemoryChannel();
            ////var c2 = new MqttInMemoryChannel();
            ////c1.Partner = c2;
            ////c2.Partner = c1;

            ////Assert.IsFalse(c1.DataAvailable);
            ////Assert.IsFalse(c2.DataAvailable);

            ////c1.Send(new byte[] { 129 });

            ////Assert.IsFalse(c1.DataAvailable);
            ////Assert.IsTrue(c2.DataAvailable);

            ////var buffer = new byte[2];
            ////var receivedBytes = c2.Receive(buffer);

            ////Assert.AreEqual(1, receivedBytes);
            ////Assert.AreEqual(129, buffer[0]);

            ////Assert.IsFalse(c1.DataAvailable);
            ////Assert.IsFalse(c2.DataAvailable);
        }

        [TestMethod]
        public void Mqtt_Packages()
        {
            ////var c1 = new MqttInMemoryChannel();
            ////var c2 = new MqttInMemoryChannel();
            ////c1.Partner = c2;
            ////c2.Partner = c1;

            ////var sendBuffer1 = new byte[] { 10, 11, 19, 85 };
            ////var sendBuffer2 = new byte[] { 1, 2, 3, 4 };
            
            ////c1.Send(sendBuffer1);
            ////c1.Send(sendBuffer2);

            ////Assert.IsFalse(c1.DataAvailable);
            ////Assert.IsTrue(c2.DataAvailable);

            ////var receiveBuffer1 = new byte[40];
            ////var receiveBuffer2 = new byte[40];

            ////var readBytes1 = c2.Receive(receiveBuffer1);
            ////Assert.IsTrue(c2.DataAvailable);
            ////var readBytes2 = c2.Receive(receiveBuffer2);
            ////Assert.IsFalse(c1.DataAvailable);

            ////Assert.AreEqual(sendBuffer1.Length, readBytes1);
            ////Assert.AreEqual(sendBuffer2.Length, readBytes2);

            ////Array.Resize(ref receiveBuffer1, readBytes1);
            ////Array.Resize(ref receiveBuffer2, readBytes2);

            ////CollectionAssert.AreEqual(sendBuffer1, receiveBuffer1);
            ////CollectionAssert.AreEqual(sendBuffer2, receiveBuffer2);
        }
    }
}
