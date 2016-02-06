using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.I2CHardwareBridge
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

        public override void Execute(II2CDevice i2CDevice)
        {
            i2CDevice.Write(GenerateRegisterSensorPackage());

            byte[] buffer = new byte[9];
            i2CDevice.Read(buffer);

            ParseResponse(buffer);
        }

        private void ParseResponse(byte[] buffer)
        {
            float temperature = 0.0F;
            float humidity = 0.0F;

            bool succeeded = buffer[0] == 1;

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
