using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Scripting;
using MoonSharp.Interpreter;

namespace HA4IoT.Hardware.I2C
{
    public class I2CBusScriptProxy : IScriptProxy
    {
        private readonly II2CBusService _i2CBusService;

        [MoonSharpHidden]
        public I2CBusScriptProxy(II2CBusService i2CBusService)
        {
            _i2CBusService = i2CBusService ?? throw new ArgumentNullException(nameof(i2CBusService));
        }

        [MoonSharpHidden]
        public string Name => "i2c";

        public bool TestAddress(int address)
        {
            var i2CAddress = new I2CSlaveAddress(address);
            var result = _i2CBusService.Read(i2CAddress, new byte[1]);
            return result.Status != I2CTransferStatus.SlaveAddressNotAcknowledged;
        }

        public void Write(int address, byte[] buffer)
        {
            _i2CBusService.Write(new I2CSlaveAddress(address), buffer);
        }

        public byte[] WriteRead(int address, byte[] writeBuffer, int length)
        {
            var readBuffer = new byte[length];
            _i2CBusService.WriteRead(new I2CSlaveAddress(address), writeBuffer, readBuffer);
            return readBuffer;
        }

        public byte[] Read(int address, int length)
        {
            var buffer = new byte[length];
            _i2CBusService.Read(new I2CSlaveAddress(address), buffer);
            return buffer;
        }
    }
}
