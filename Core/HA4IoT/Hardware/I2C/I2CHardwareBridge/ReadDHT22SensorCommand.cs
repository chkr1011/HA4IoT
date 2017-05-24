using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.I2C.I2CHardwareBridge
{
    public class ReadDHT22SensorCommand : I2CHardwareBridgeCommand
    {
        private const byte I2C_ACTION_DHT22 = 1;
        private const byte ACTION_REGISTER_SENSOR = 0;

        private byte _pin;

        public ReadDHT22SensorCommandResponse Response { get; private set; }

        public ReadDHT22SensorCommand WithPin(byte pin)
        {
            _pin = pin;
            return this;
        }

        public override void Execute(I2CSlaveAddress address, II2CBusService i2CBusService)
        {
            i2CBusService.Write(address, GenerateRegisterSensorPackage(), false);

            var buffer = new byte[9];
            i2CBusService.Read(address, buffer, false);

            ParseResponse(buffer);
        }

        private void ParseResponse(byte[] buffer)
        {
            var temperature = 0.0F;
            var humidity = 0.0F;

            var succeeded = buffer[0] == 1;

            if (succeeded)
            {
                temperature = BitConverter.ToSingle(buffer, 1);
                humidity = BitConverter.ToSingle(buffer, 5);
            }

            Response = new ReadDHT22SensorCommandResponse(succeeded, temperature, humidity);
        }

        private byte[] GenerateRegisterSensorPackage()
        {
            return new[]
            {
                I2C_ACTION_DHT22,
                ACTION_REGISTER_SENSOR,
                _pin
            };
        }
    }
}
