using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.I2C.I2CHardwareBridge
{
    public class SendInfraredSignalCommand : I2CHardwareBridgeCommand
    {
        private const byte I2C_ACTION_Infrared = 3;
        private const byte ACTION_RESET_SIGNAL_CACHE = 0;
        private const byte ACTION_FILL_SIGNAL_CACHE = 1;
        private const byte ACTION_SEND_CACHED_SIGNAL = 2;

        private byte[] _signal;
        private byte _pin;
        private byte _repeats = 1;
        private byte _frequency = 26;

        public SendInfraredSignalCommand WithSignal(byte[] signal)
        {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (signal.Length > 128) throw new NotSupportedException("Signal is longer than 128 bytes.");

            _signal = signal;
            return this;
        }

        public SendInfraredSignalCommand WithPin(byte pin)
        {
            _pin = pin;
            return this;
        }

        public SendInfraredSignalCommand WithRepeats(byte count)
        {
            _repeats = count;
            return this;
        }

        public SendInfraredSignalCommand WithFrequency(byte frequency)
        {
            _frequency = frequency;
            return this;
        }

        public override void Execute(I2CSlaveAddress address, II2CBusService i2CBusService)
        {
            i2CBusService.Write(address, GenerateClearSignalCachePackage(), false);

            var offset = 0;
            while (offset < _signal.Length)
            {
                var buffer = _signal.Skip(offset).Take(30).ToArray();
                offset += buffer.Length;

                i2CBusService.Write(address, GenerateFillSignalCachePackage(buffer), false);
            }

            i2CBusService.Write(address, GenerateSendCachedSignalPackage(), false);
        }

        private byte[] GenerateSendCachedSignalPackage()
        {
            return new[]
            {
                    I2C_ACTION_Infrared,
                    ACTION_SEND_CACHED_SIGNAL,
                    _pin,
                    _frequency,
                    _repeats
                };
        }

        private static byte[] GenerateFillSignalCachePackage(byte[] data)
        {
            var buffer = new List<byte>
            {
                I2C_ACTION_Infrared,
                ACTION_FILL_SIGNAL_CACHE
            };

            buffer.AddRange(data);

            return buffer.ToArray();
        }

        private static byte[] GenerateClearSignalCachePackage()
        {
            return new[]
            {
                    I2C_ACTION_Infrared,
                    ACTION_RESET_SIGNAL_CACHE
                };
        }
    }
}
