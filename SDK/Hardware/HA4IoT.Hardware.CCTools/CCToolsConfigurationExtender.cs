using System;
using System.Xml.Linq;
using HA4IoT.Configuration;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsConfigurationExtender : IConfigurationExtender
    {
        private readonly IController _controller;

        public string Namespace { get; } = "http://www.ha4iot.de/ConfigurationExtenders/CCTools";

        public CCToolsConfigurationExtender(IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            _controller = controller;
        }

        public IDevice ParseDevice(XElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            switch (element.Name.LocalName)
            {
                case "HSPE16InputOnly": return ParseHSPE16InputOnly(element);
                case "HSPE16OutputOnly": return ParseHSPE16InputOnly(element);

                case "HSPE8InputOnly": return ParseHSPE8InputOnly(element);
                case "HSPE8OutputOnly": return ParseHSPE8OutputOnly(element);

                case "HSRel5": return ParseHSRel5(element);
                case "HSRel8": return ParseHSRel8(element);
                
                case "HSRT16": return ParseHSRT16(element);

                default: throw new ConfigurationInvalidException("Device not supported.", element);
            }
        }

        public IBinaryOutput ParseBinaryOutput(XElement element)
        {
            throw new NotImplementedException();
        }

        public IBinaryInput ParseBinaryInput(XElement element)
        {
            throw new NotImplementedException();
        }

        public IActuator ParseActuator(XElement element)
        {
            throw new NotImplementedException();
        }

        public void OnConfigurationParsed()
        {
        }

        public void OnInitializationFromCodeCompleted()
        {
        }

        private HSREL5 ParseHSRel5(XElement element)
        {
            return new HSREL5(
                new DeviceId(element.GetMandatoryStringFromAttribute("id")), 
                new I2CSlaveAddress(element.GetMandatoryIntFromAttribute("i2cAddress")),
                _controller.GetDevice<II2CBus>(element.GetStringFromAttribute("i2cBus", "II2CBus.default").ToDeviceId()),
                _controller.HttpApiController,
                _controller.Logger);
        }

        private HSREL8 ParseHSRel8(XElement element)
        {
            return new HSREL8(
                new DeviceId(element.GetMandatoryStringFromAttribute("id")),
                new I2CSlaveAddress(element.GetMandatoryIntFromAttribute("i2cAddress")),
                _controller.GetDevice<II2CBus>(element.GetStringFromAttribute("i2cBus", "II2CBus.default").ToDeviceId()),
                _controller.HttpApiController,
                _controller.Logger);
        }

        private HSPE16OutputOnly ParseHSPE16OutputOnly(XElement element)
        {
            return new HSPE16OutputOnly(
                new DeviceId(element.GetMandatoryStringFromAttribute("id")),
                new I2CSlaveAddress(element.GetMandatoryIntFromAttribute("i2cAddress")),
                _controller.GetDevice<II2CBus>(element.GetStringFromAttribute("i2cBus", "II2CBus.default").ToDeviceId()),
                _controller.HttpApiController,
                _controller.Logger);
        }

        private HSPE16InputOnly ParseHSPE16InputOnly(XElement element)
        {
            return new HSPE16InputOnly(
                new DeviceId(element.GetMandatoryStringFromAttribute("id")),
                new I2CSlaveAddress(element.GetMandatoryIntFromAttribute("i2cAddress")),
                _controller.GetDevice<II2CBus>(element.GetStringFromAttribute("i2cBus", "II2CBus.default").ToDeviceId()),
                _controller.HttpApiController,
                _controller.Logger);
        }

        private HSPE8OutputOnly ParseHSPE8OutputOnly(XElement element)
        {
            return new HSPE8OutputOnly(
                new DeviceId(element.GetMandatoryStringFromAttribute("id")),
                new I2CSlaveAddress(element.GetMandatoryIntFromAttribute("i2cAddress")),
                _controller.GetDevice<II2CBus>(element.GetStringFromAttribute("i2cBus", "II2CBus.default").ToDeviceId()),
                _controller.HttpApiController,
                _controller.Logger);
        }

        private HSPE8InputOnly ParseHSPE8InputOnly(XElement element)
        {
            return new HSPE8InputOnly(
                new DeviceId(element.GetMandatoryStringFromAttribute("id")),
                new I2CSlaveAddress(element.GetMandatoryIntFromAttribute("i2cAddress")),
                _controller.GetDevice<II2CBus>(element.GetStringFromAttribute("i2cBus", "II2CBus.default").ToDeviceId()),
                _controller.HttpApiController,
                _controller.Logger);
        }

        private HSRT16 ParseHSRT16(XElement element)
        {
            return new HSRT16(
                new DeviceId(element.GetMandatoryStringFromAttribute("id")),
                new I2CSlaveAddress(element.GetMandatoryIntFromAttribute("i2cAddress")),
                _controller.GetDevice<II2CBus>(element.GetStringFromAttribute("i2cBus", "II2CBus.default").ToDeviceId()),
                _controller.HttpApiController,
                _controller.Logger);
        }
    }
}
