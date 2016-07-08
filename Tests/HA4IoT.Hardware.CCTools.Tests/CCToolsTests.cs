using FluentAssertions;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Hardware.CCTools.Tests
{
    [TestClass]
    public class CCToolsTests
    {
        [TestMethod]
        public void CCTools_Write_HSRel5_State()
        {
            var i2cBus = new TestI2CBus(DeviceIdFactory.EmptyId);
            var hsrel5 = new HSREL5(DeviceIdFactory.EmptyId, new I2CSlaveAddress(66), i2cBus);
            hsrel5[HSREL5Pin.Relay0].Write(BinaryState.High);

            i2cBus.LastUsedI2CSlaveAddress.ShouldBeEquivalentTo(new I2CSlaveAddress(66));
            i2cBus.I2CDevice.LastWrittenBytes.Length.ShouldBeEquivalentTo(1);

            // The bits are inverted using a hardware inverter. This requires checking
            // against inverted values too.
            i2cBus.I2CDevice.LastWrittenBytes[0].ShouldBeEquivalentTo(254);

            hsrel5[HSREL5Pin.Relay4].Write(BinaryState.High);
            i2cBus.LastUsedI2CSlaveAddress.ShouldBeEquivalentTo(new I2CSlaveAddress(66));
            i2cBus.I2CDevice.LastWrittenBytes.Length.ShouldBeEquivalentTo(1);

            // The bits are inverted using a hardware inverter. This requires checking
            // against inverted values too.
            i2cBus.I2CDevice.LastWrittenBytes[0].ShouldBeEquivalentTo(238);
        }

        [TestMethod]
        public void CCTools_Read_HSPE16InputOnly_State()
        {
            // The hardware board contains pull-up resistors. This means that the inputs are inverted internally
            // and the test must respect this.
            var i2cBus = new TestI2CBus(DeviceIdFactory.EmptyId);
            i2cBus.I2CDevice.BufferForNextRead = new byte[] { 255, 255 };

            var hspe16 = new HSPE16InputOnly(DeviceIdFactory.EmptyId, new I2CSlaveAddress(32), i2cBus);

            hspe16.FetchState();
            i2cBus.LastUsedI2CSlaveAddress.ShouldBeEquivalentTo(new I2CSlaveAddress(32));
            hspe16[HSPE16Pin.GPIO0].Read().ShouldBeEquivalentTo(BinaryState.Low);

            i2cBus.I2CDevice.BufferForNextRead = new byte[] { 0, 255 };
            hspe16.FetchState();
            i2cBus.LastUsedI2CSlaveAddress.ShouldBeEquivalentTo(new I2CSlaveAddress(32));
            hspe16[HSPE16Pin.GPIO0].Read().ShouldBeEquivalentTo(BinaryState.High);

            i2cBus.I2CDevice.BufferForNextRead = new byte[] { 255, 127 };
            hspe16.FetchState();
            i2cBus.LastUsedI2CSlaveAddress.ShouldBeEquivalentTo(new I2CSlaveAddress(32));
            hspe16[HSPE16Pin.GPIO0].Read().ShouldBeEquivalentTo(BinaryState.Low);
            hspe16[HSPE16Pin.GPIO15].Read().ShouldBeEquivalentTo(BinaryState.High);
        }
    }
}
