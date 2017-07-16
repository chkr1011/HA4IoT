using HA4IoT.Contracts.Hardware;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Hardware
{
    [TestClass]
    public class TestPortTests
    {
        [TestMethod]
        public void TestPort_Read_ShouldReturn_LOW()
        {
            var p = new TestPort(BinaryState.Low);
            Assert.AreEqual(BinaryState.Low, p.Read());
        }

        [TestMethod]
        public void TestPort_Read_ShouldReturn_High()
        {
            var p = new TestPort(BinaryState.High);
            Assert.AreEqual(BinaryState.High, p.Read());
        }

        [TestMethod]
        public void TestPort_Read_Inverted_ShouldReturn_High()
        {
            var p = new InvertedBinaryInput(new TestPort(BinaryState.Low));
            Assert.AreEqual(BinaryState.High, p.Read());
        }

        [TestMethod]
        public void TestPort_Read_Inverted_ShouldReturn_Low()
        {
            var p = new InvertedBinaryInput(new TestPort(BinaryState.High));
            Assert.AreEqual(BinaryState.Low, p.Read());
        }

        [TestMethod]
        public void TestPort_WriteRead_Inverted_ShouldReturn_High()
        {
            var p = new InvertedBinarOutput(new TestPort(BinaryState.Low));
            p.Write(BinaryState.High);
            Assert.AreEqual(BinaryState.High, p.Read());
        }

        [TestMethod]
        public void TestPort_WriteRead_Inverted_ShouldReturn_Low()
        {
            var p = new InvertedBinarOutput(new TestPort(BinaryState.High));
            p.Write(BinaryState.Low);
            Assert.AreEqual(BinaryState.Low, p.Read());
        }

        [TestMethod]
        public void TestPort_Read_DoubleInverted_ShouldReturn_Low()
        {
            var p = new InvertedBinarOutput(new InvertedBinarOutput(new TestPort(BinaryState.Low)));
            p.Write(BinaryState.Low);
            Assert.AreEqual(BinaryState.Low, p.Read());
        }
    }
}
