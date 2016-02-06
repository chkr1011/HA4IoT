using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.I2CHardwareBridge
{
    public class SendLDP433MhzSignalCommand : I2CHardwareBridgeCommand
    {
        private const byte I2C_ACTION_433MHz = 2;

        private uint _code;
        private byte _length = 24;
        private byte _pin;
        private byte _repeats = 10;

        public SendLDP433MhzSignalCommand WithCode(uint code)
        {
            _code = code;
            return this;
        }

        public SendLDP433MhzSignalCommand WithLength(byte length)
        {
            _length = length;
            return this;
        }

        public SendLDP433MhzSignalCommand WithPin(byte pin)
        {
            _pin = pin;
            return this;
        }

        public SendLDP433MhzSignalCommand WithRepeats(byte count)
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

            package[5] = _length;
            package[6] = _repeats;
            package[7] = _pin;

            return package;
        }
    }
}
