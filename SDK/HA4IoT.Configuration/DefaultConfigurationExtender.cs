using System.Xml.Linq;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Configuration
{
    public class DefaultConfigurationExtender : IConfigurationExtender
    {
        public string Namespace { get; }

        public IDevice ParseDevice(XElement element)
        {
            throw new System.NotImplementedException();
        }

        public IBinaryOutput ParseBinaryOutput(XElement element)
        {
            throw new System.NotImplementedException();
        }

        public IBinaryInput ParseBinaryInput(XElement element)
        {
            throw new System.NotImplementedException();
        }
    }
}
