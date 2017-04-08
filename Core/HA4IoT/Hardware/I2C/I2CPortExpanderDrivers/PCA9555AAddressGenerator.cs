using System;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.I2C.I2CPortExpanderDrivers
{
    public static class PCA9555DAddressGenerator
    {
        public static I2CSlaveAddress Generate(AddressPinState a0, AddressPinState a1, AddressPinState a2)
        {
            if (a0 == AddressPinState.Low && a1 == AddressPinState.Low && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x38);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.Low && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x39);
            }

            if (a0 == AddressPinState.Low && a1 == AddressPinState.High && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x3A);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.High && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x3B);
            }

            if (a0 == AddressPinState.Low && a1 == AddressPinState.Low && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x3C);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.Low && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x3D);
            }

            if (a0 == AddressPinState.Low && a1 == AddressPinState.High && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x3E);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.High && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x3F);
            }

            throw new NotSupportedException();
        }
    }
}
