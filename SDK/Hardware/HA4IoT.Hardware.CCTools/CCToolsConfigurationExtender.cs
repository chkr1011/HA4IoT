using System;
using System.Xml.Linq;
using HA4IoT.Configuration;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsConfigurationExtender : ConfigurationExtenderBase
    {
        public CCToolsConfigurationExtender(ConfigurationParser parser, IController controller) 
            : base(parser, controller)
        {
            Namespace = "http://www.ha4iot.de/ConfigurationExtenders/CCTools";
        }

        public override IDevice ParseDevice(XElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            switch (element.Name.LocalName)
            {
                case "HSPE16InputOnly": return ParseHSPE16InputOnly(element);
                case "HSPE16OutputOnly": return ParseHSPE16OutputOnly(element);

                case "HSPE8InputOnly": return ParseHSPE8InputOnly(element);
                case "HSPE8OutputOnly": return ParseHSPE8OutputOnly(element);

                case "HSRel5": return ParseHSRel5(element);
                case "HSRel8": return ParseHSRel8(element);
                
                case "HSRT16": return ParseHSRT16(element);

                default: throw new ConfigurationInvalidException("Device not supported.", element);
            }
        }

        public override IBinaryInput ParseBinaryInput(XElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            var device = Controller.Device<CCToolsBoardBase>(new DeviceId(element.GetMandatoryStringFromAttribute("deviceId")));
            var port = element.GetMandatoryStringFromAttribute("port");

            var hspe16 = device as HSPE16InputOnly;
            if (hspe16 != null)
            {
                return hspe16[(HSPE16Pin) Enum.Parse(typeof (HSPE16Pin), port)];
            }

            var hspe8 = device as HSPE8InputOnly;
            if (hspe8 != null)
            {
                return hspe8[(HSPE8Pin)Enum.Parse(typeof (HSPE8Pin), port)];
            }

            throw new ConfigurationInvalidException("CCTools device '" + device.GetType().FullName + "' is no input device.", element);
        }

        public override IBinaryOutput ParseBinaryOutput(XElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            var device = Controller.Device<CCToolsBoardBase>(new DeviceId(element.GetMandatoryStringFromAttribute("deviceId")));
            var port = element.GetMandatoryStringFromAttribute("port");

            var hspe16 = device as HSPE16OutputOnly;
            if (hspe16 != null)
            {
                return hspe16[(HSPE16Pin)Enum.Parse(typeof(HSPE16Pin), port)];
            }

            var hspe8 = device as HSPE8OutputOnly;
            if (hspe8 != null)
            {
                return hspe8[(HSPE8Pin)Enum.Parse(typeof(HSPE8Pin), port)];
            }

            var hsrel8 = device as HSREL8;
            if (hsrel8 != null)
            {
                return hsrel8[(HSREL8Pin)Enum.Parse(typeof(HSREL8Pin), port)];
            }

            var hsrel5 = device as HSREL5;
            if (hsrel5 != null)
            {
                return hsrel5[(HSREL5Pin)Enum.Parse(typeof(HSREL5Pin), port)];
            }

            var hsrt16 = device as HSRT16;
            if (hsrt16 != null)
            {
                return hsrt16[(HSRT16Pin)Enum.Parse(typeof(HSRT16Pin), port)];
            }

            throw new ConfigurationInvalidException("CCTools device '" + device.GetType().FullName + "' is no input device.", element);
        }

        private HSREL5 ParseHSRel5(XElement element)
        {
            return new HSREL5(
                new DeviceId(element.GetMandatoryStringFromAttribute("id")), 
                new I2CSlaveAddress(element.GetMandatoryIntFromAttribute("i2cAddress")),
                Controller.Device<II2CBus>(element.GetStringFromAttribute("i2cBus", "II2CBus.default").ToDeviceId()),
                Controller.HttpApiController,
                Controller.Logger);
        }

        private HSREL8 ParseHSRel8(XElement element)
        {
            return new HSREL8(
                new DeviceId(element.GetMandatoryStringFromAttribute("id")),
                new I2CSlaveAddress(element.GetMandatoryIntFromAttribute("i2cAddress")),
                Controller.Device<II2CBus>(element.GetStringFromAttribute("i2cBus", "II2CBus.default").ToDeviceId()),
                Controller.HttpApiController,
                Controller.Logger);
        }

        private HSPE16OutputOnly ParseHSPE16OutputOnly(XElement element)
        {
            return new HSPE16OutputOnly(
                new DeviceId(element.GetMandatoryStringFromAttribute("id")),
                new I2CSlaveAddress(element.GetMandatoryIntFromAttribute("i2cAddress")),
                Controller.Device<II2CBus>(element.GetStringFromAttribute("i2cBus", "II2CBus.default").ToDeviceId()),
                Controller.HttpApiController,
                Controller.Logger);
        }

        private HSPE16InputOnly ParseHSPE16InputOnly(XElement element)
        {
            return new HSPE16InputOnly(
                new DeviceId(element.GetMandatoryStringFromAttribute("id")),
                new I2CSlaveAddress(element.GetMandatoryIntFromAttribute("i2cAddress")),
                Controller.Device<II2CBus>(element.GetStringFromAttribute("i2cBus", "II2CBus.default").ToDeviceId()),
                Controller.HttpApiController,
                Controller.Logger);
        }

        private HSPE8OutputOnly ParseHSPE8OutputOnly(XElement element)
        {
            return new HSPE8OutputOnly(
                new DeviceId(element.GetMandatoryStringFromAttribute("id")),
                new I2CSlaveAddress(element.GetMandatoryIntFromAttribute("i2cAddress")),
                Controller.Device<II2CBus>(element.GetStringFromAttribute("i2cBus", "II2CBus.default").ToDeviceId()),
                Controller.HttpApiController,
                Controller.Logger);
        }

        private HSPE8InputOnly ParseHSPE8InputOnly(XElement element)
        {
            return new HSPE8InputOnly(
                new DeviceId(element.GetMandatoryStringFromAttribute("id")),
                new I2CSlaveAddress(element.GetMandatoryIntFromAttribute("i2cAddress")),
                Controller.Device<II2CBus>(element.GetStringFromAttribute("i2cBus", "II2CBus.default").ToDeviceId()),
                Controller.HttpApiController,
                Controller.Logger);
        }

        private HSRT16 ParseHSRT16(XElement element)
        {
            return new HSRT16(
                new DeviceId(element.GetMandatoryStringFromAttribute("id")),
                new I2CSlaveAddress(element.GetMandatoryIntFromAttribute("i2cAddress")),
                Controller.Device<II2CBus>(element.GetStringFromAttribute("i2cBus", "II2CBus.default").ToDeviceId()),
                Controller.HttpApiController,
                Controller.Logger);
        }
    }
}
