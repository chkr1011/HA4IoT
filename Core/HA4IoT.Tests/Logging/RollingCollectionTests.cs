using System.Linq;
using HA4IoT.Logging;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Logging
{
    [TestClass]
    public class RollingCollectionTests
    {
        [TestMethod]
        public void RollingCollection_Add()
        {
            var c = new RollingCollection<string>(10);
            Assert.AreEqual(0, c.Count);
            c.Add("a");
            Assert.AreEqual(1, c.Count);

            var cc = c.ToList();
            Assert.AreEqual(1, cc.Count);
            Assert.AreEqual("a", cc[0]);
        }

        [TestMethod]
        public void RollingCollection_With_Overflow()
        {
            var c = new RollingCollection<string>(3);
            Assert.AreEqual(0, c.Count);
            c.Add("a");
            Assert.AreEqual(1, c.Count);
            c.Add("b");
            Assert.AreEqual(2, c.Count);
            c.Add("c");
            Assert.AreEqual(3, c.Count);

            c.Add("d");
            Assert.AreEqual(3, c.Count);

            Assert.AreEqual("d", c[2]);
        }
    }
}
