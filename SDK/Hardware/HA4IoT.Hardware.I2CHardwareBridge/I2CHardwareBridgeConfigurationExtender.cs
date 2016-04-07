using System;
using System.Xml.Linq;
using HA4IoT.Configuration;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Hardware.I2CHardwareBridge
{
    public class I2CHardwareBridgeConfigurationExtender : ConfigurationExtenderBase
    {
        public I2CHardwareBridgeConfigurationExtender(ConfigurationParser parser, IController controller) 
            : base(parser, controller)
        {
            Namespace = "http://www.ha4iot.de/ConfigurationExtenders/I2CHardwareBridge";
        }

        public override IDevice ParseDevice(XElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            switch (element.Name.LocalName)
            {
                case "I2CHardwareBridge": return ParseI2CHardwareBridge(element);
                
                default: throw new ConfigurationInvalidException("Device not supported.", element);
            }
        }

        public override INumericValueSensorEndpoint ParseNumericValueSensor(XElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            switch (element.Name.LocalName)
            {
                case "TemperatureSensor": return ParseTemperatureSensor(element);
                case "HumiditySensor": return ParseHumiditySensor(element);

                default: throw new ConfigurationInvalidException("Sensor not supported.", element);
            }
        }

        private INumericValueSensorEndpoint ParseHumiditySensor(XElement element)
        {
            var i2cHardwareBridge =
                Controller.GetDevice<I2CHardwareBridge>(
                    new DeviceId(element.GetMandatoryStringFromAttribute("i2cHardwareBridgeDeviceId")));

            return i2cHardwareBridge.DHT22Accessor.GetHumiditySensor((byte)element.GetMandatoryIntFromAttribute("sensorId"));
        }

        private INumericValueSensorEndpoint ParseTemperatureSensor(XElement element)
        {
            var i2cHardwareBridge =
                Controller.GetDevice<I2CHardwareBridge>(
                    new DeviceId(element.GetMandatoryStringFromAttribute("i2cHardwareBridgeDeviceId")));

            return i2cHardwareBridge.DHT22Accessor.GetTemperatureSensor((byte)element.GetMandatoryIntFromAttribute("sensorId"));
        }

        private IDevice ParseI2CHardwareBridge(XElement element)
        {
            return new I2CHardwareBridge(
                new DeviceId(element.GetMandatoryStringFromAttribute("id")),
                new I2CSlaveAddress(element.GetMandatoryIntFromAttribute("i2cAddress")),
                Controller.GetDevice<II2CBus>(new DeviceId(element.GetStringFromAttribute("i2cBus", "II2CBus.default"))),
                Controller.Timer);
        }
    }
}
