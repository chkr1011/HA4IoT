using System;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.I2C.I2CHardwareBridge
{
    public class SendLDP433MhzSignalCommand : I2CHardwareBridgeCommand
    {
        private const byte I2C_ACTION_433MHz = 2;

        private uint _code;
        private int _length = 24;
        private int _pin;
        private int _repeats = 10;

        public SendLDP433MhzSignalCommand WithCode(uint code)
        {
            _code = code;
            return this;
        }

        public SendLDP433MhzSignalCommand WithLength(int length)
        {
            _length = length;
            return this;
        }

        public SendLDP433MhzSignalCommand WithPin(int pin)
        {
            _pin = pin;
            return this;
        }

        public SendLDP433MhzSignalCommand WithRepeats(int count)
        {
            _repeats = count;
            return this;
        }

        public override void Execute(II2CDevice i2CDevice)
        {
            i2CDevice.Write(ToPackage());
        }

        private byte[] ToPackage()
        {
            var package = new byte[8];
            package[0] = I2C_ACTION_433MHz;

            var code = BitConverter.GetBytes(_code);
            Array.Copy(code, 0, package, 1, 4);

            package[5] = (byte)_length;
            package[6] = (byte)_repeats;
            package[7] = (byte)_pin;

            return package;
        }
    }
}
