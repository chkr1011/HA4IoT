using System.Xml.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
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

        ISingleValueSensor ParseSingleValueSensor(XElement element);

        IActuator ParseActuator(XElement element);

        void OnConfigurationParsed();

        void OnInitializationFromCodeCompleted();
    }
}
