using HA4IoT.Core;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests
{
    [TestClass]
    public static class TestInitializer
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            Controller.IsRunningInUnitTest = true;
        }
    }
}
