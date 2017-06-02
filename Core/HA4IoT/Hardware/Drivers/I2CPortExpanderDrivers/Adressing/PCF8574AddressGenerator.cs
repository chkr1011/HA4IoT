using System;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.Drivers.I2CPortExpanderDrivers.Adressing
{
    public static class PCF8574AddressGenerator
    {
        public static I2CSlaveAddress Generate(AddressPinState a0, AddressPinState a1, AddressPinState a2)
        {
            if (a0 == AddressPinState.Low && a1 == AddressPinState.Low && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x20);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.Low && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x21);
            }

            if (a0 == AddressPinState.Low && a1 == AddressPinState.High && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x22);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.High && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x23);
            }

            if (a0 == AddressPinState.Low && a1 == AddressPinState.Low && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x24);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.Low && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x25);
            }

            if (a0 == AddressPinState.Low && a1 == AddressPinState.High && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x26);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.High && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x27);
            }

            throw new NotSupportedException();
        }
    }
}
