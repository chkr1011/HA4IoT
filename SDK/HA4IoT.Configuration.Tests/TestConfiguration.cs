using System.Reflection;
using System.Xml.Linq;

namespace HA4IoT.Configuration.Tests
{
    internal static class TestConfiguration
    {
        public static XDocument GetConfiguration()
        {
            using (var stream = typeof(ConfigurationParserDevicesTests).GetTypeInfo().Assembly.GetManifestResourceStream("HA4IoT.Configuration.Tests.Configuration.xml"))
            {
                return XDocument.Load(stream);
            }
        }
    }
}
