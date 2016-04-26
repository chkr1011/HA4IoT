using System.Xml.Linq;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Configuration
{
    public interface IConfigurationExtender
    {
        string Namespace { get; }

        IService ParseService(XElement element);

        IDevice ParseDevice(XElement element);

        IBinaryOutput ParseBinaryOutput(XElement element);

        IBinaryInput ParseBinaryInput(XElement element);

        INumericValueSensorEndpoint ParseNumericValueSensor(XElement element);

        IComponent ParseComponent(XElement element);

        void OnConfigurationParsed();

        void OnInitializationFromCodeCompleted();
    }
}
