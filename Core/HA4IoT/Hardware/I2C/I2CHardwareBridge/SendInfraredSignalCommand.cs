using System;
using System.Collections.Generic;
using System.Linq;
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

        public override void Execute(II2CDevice i2CDevice)
        {
            i2CDevice.Write(GenerateClearSignalCachePackage());

            int offset = 0;
            while (offset < _signal.Length)
            {
                var buffer = _signal.Skip(offset).Take(30).ToArray();
                offset += buffer.Length;

                i2CDevice.Write(GenerateFillSignalCachePackage(buffer));
            }

            i2CDevice.Write(GenerateSendCachedSignalPackage());
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

        private byte[] GenerateFillSignalCachePackage(byte[] data)
        {
            var buffer = new List<byte>();
            buffer.Add(I2C_ACTION_Infrared);
            buffer.Add(ACTION_FILL_SIGNAL_CACHE);
            buffer.AddRange(data);

            return buffer.ToArray();
        }

        private byte[] GenerateClearSignalCachePackage()
        {
            return new[]
            {
                    I2C_ACTION_Infrared,
                    ACTION_RESET_SIGNAL_CACHE
                };
        }
    }
}
