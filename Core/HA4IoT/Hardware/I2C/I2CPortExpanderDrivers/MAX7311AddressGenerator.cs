using System;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.I2C.I2CPortExpanderDrivers
{
    public static class MAX7311AddressGenerator
    {
        public static I2CSlaveAddress Generate(AddressPinState a0, AddressPinState a1, AddressPinState a2)
        {
            // Block 1
            if (a0 == AddressPinState.Low && a1 == AddressPinState.SCL && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x10);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.SCL && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x11);
            }

            if (a0 == AddressPinState.Low && a1 == AddressPinState.SDA && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x12);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.SDA && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x13);
            }

            if (a0 == AddressPinState.Low && a1 == AddressPinState.SCL && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x14);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.SCL && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x15);
            }

            if (a0 == AddressPinState.Low && a1 == AddressPinState.SDA && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x16);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.SDA && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x17);
            }

            // Block 2
            if (a0 == AddressPinState.Low && a1 == AddressPinState.SCL && a2 == AddressPinState.SCL)
            {
                return new I2CSlaveAddress(0x18);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.SCL && a2 == AddressPinState.SDA)
            {
                return new I2CSlaveAddress(0x19);
            }

            if (a0 == AddressPinState.Low && a1 == AddressPinState.SDA && a2 == AddressPinState.SCL)
            {
                return new I2CSlaveAddress(0x1A);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.SDA && a2 == AddressPinState.SDA)
            {
                return new I2CSlaveAddress(0x1B);
            }

            if (a0 == AddressPinState.Low && a1 == AddressPinState.SCL && a2 == AddressPinState.SCL)
            {
                return new I2CSlaveAddress(0x1C);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.SCL && a2 == AddressPinState.SDA)
            {
                return new I2CSlaveAddress(0x1D);
            }

            if (a0 == AddressPinState.Low && a1 == AddressPinState.SDA && a2 == AddressPinState.SCL)
            {
                return new I2CSlaveAddress(0x1E);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.SDA && a2 == AddressPinState.SDA)
            {
                return new I2CSlaveAddress(0x1F);
            }

            // Block 3
            if (a0 == AddressPinState.Low && a1 == AddressPinState.Low && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x20);
            }

            if (a0 == AddressPinState.Low && a1 == AddressPinState.Low && a2 == AddressPinState.High)
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

            if (a0 == AddressPinState.High && a1 == AddressPinState.Low && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x24);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.Low && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x25);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.High && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x26);
            }

            if (a0 == AddressPinState.High && a1 == AddressPinState.High && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x27);
            }

            // Block 4
            if (a0 == AddressPinState.SCL && a1 == AddressPinState.Low && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x28);
            }

            if (a0 == AddressPinState.SDA && a1 == AddressPinState.Low && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x29);
            }

            if (a0 == AddressPinState.SCL && a1 == AddressPinState.High && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x2A);
            }

            if (a0 == AddressPinState.SDA && a1 == AddressPinState.High && a2 == AddressPinState.Low)
            {
                return new I2CSlaveAddress(0x2B);
            }

            if (a0 == AddressPinState.SCL && a1 == AddressPinState.Low && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x2C);
            }

            if (a0 == AddressPinState.SDA && a1 == AddressPinState.Low && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x2D);
            }

            if (a0 == AddressPinState.SCL && a1 == AddressPinState.High && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x2E);
            }

            if (a0 == AddressPinState.SDA && a1 == AddressPinState.High && a2 == AddressPinState.High)
            {
                return new I2CSlaveAddress(0x2F);
            }

            // TODO: Add second page of address table from MAX7311 datasheet
            throw new NotSupportedException();
        }
    }
}
