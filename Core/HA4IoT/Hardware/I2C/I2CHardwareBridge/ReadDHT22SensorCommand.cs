using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.I2C.I2CHardwareBridge
{
    public class ReadDHT22SensorCommand : I2CHardwareBridgeCommand
    {
        private readonly byte _pin;
        private readonly byte[] _buffer = new byte[9];

        public ReadDHT22SensorCommand(byte pin)
        {
            _pin = pin;
        }

        public ReadDHT22SensorCommandResponse Response { get; } = new ReadDHT22SensorCommandResponse();

        public override void Execute(I2CSlaveAddress address, II2CBusService i2CBusService)
        {
            i2CBusService.Write(address, GenerateRegisterSensorPackage(), false);
            i2CBusService.Read(address, _buffer, false);

            ParseResponse();
        }

        private void ParseResponse()
        {
            var temperature = 0.0F;
            var humidity = 0.0F;

            var succeeded = _buffer[0] == 1;

            if (succeeded)
            {
                temperature = BitConverter.ToSingle(_buffer, 1);
                humidity = BitConverter.ToSingle(_buffer, 5);
            }

            Response.Succeeded = succeeded;
            Response.Temperature = temperature;
            Response.Humidity = humidity;
        }

        private byte[] GenerateRegisterSensorPackage()
        {
            return new byte[]
            {
                0x1, // Action: DHT22
                0x0, // Action: RegisterSensor
                _pin
            };
        }
    }
}
