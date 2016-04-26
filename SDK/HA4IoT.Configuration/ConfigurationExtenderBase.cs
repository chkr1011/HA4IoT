using System;
using System.Xml.Linq;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Configuration
{
    public abstract class ConfigurationExtenderBase : IConfigurationExtender
    {
        protected ConfigurationExtenderBase(ConfigurationParser parser, IController controller)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            Parser = parser;
            Controller = controller;
        }

        public string Namespace { get; protected set; } = string.Empty;

        public virtual IDevice ParseDevice(XElement element)
        {
            throw new NotSupportedException("Configuratio extender '" + Namespace + "' does not support any devices.");
        }

        public virtual IService ParseService(XElement element)
        {
            throw new NotSupportedException("Configuratio extender '" + Namespace + "' does not support any services.");
        }

        public virtual IBinaryOutput ParseBinaryOutput(XElement element)
        {
            throw new NotSupportedException("Configuratio extender '" + Namespace + "' does not support any binary outputs.");
        }

        public virtual IBinaryInput ParseBinaryInput(XElement element)
        {
            throw new NotSupportedException("Configuratio extender '" + Namespace + "' does not support any binary inputs.");
        }

        public virtual INumericValueSensorEndpoint ParseNumericValueSensor(XElement element)
        {
            throw new NotSupportedException("Configuratio extender '" + Namespace + "' does not support any single value sensors.");
        }

        public virtual IComponent ParseComponent(XElement element)
        {
            throw new NotSupportedException("Configuratio extender '" + Namespace + "' does not support any components.");
        }

        public virtual void OnConfigurationParsed()
        {
        }

        public virtual void OnInitializationFromCodeCompleted()
        {
        }

        protected ConfigurationParser Parser { get; }

        protected IController Controller { get; }
    }
}
