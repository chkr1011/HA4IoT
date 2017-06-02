using HA4IoT.Contracts.Hardware.RemoteSockets.Protocols;
using HA4IoT.Hardware.Drivers.RemoteSockets;
using HA4IoT.Hardware.RemoteSockets;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Hardware.RemoteSwitch
{
    [TestClass]
    public class DipswitchCodeProviderTests
    {
        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitAAndTurnOn()
        {
            var code = DipswitchCodeProvider.GetCode(
                DipswitchSystemCode.AllOn,
                DipswitchUnitCode.A,
                RemoteSocketCommand.TurnOn);

            Assert.AreEqual(1361U, code.Value);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitAAndTurnOff()
        {
            var code = DipswitchCodeProvider.GetCode(
                DipswitchSystemCode.AllOn,
                DipswitchUnitCode.A,
                RemoteSocketCommand.TurnOff);

            Assert.AreEqual(1364U, code.Value);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitBAndTurnOn()
        {
            var code = DipswitchCodeProvider.GetCode(
                DipswitchSystemCode.AllOn,
                DipswitchUnitCode.B,
                RemoteSocketCommand.TurnOn);

            Assert.AreEqual(4433U, code.Value);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitBAndTurnOff()
        {
            var code = DipswitchCodeProvider.GetCode(
                DipswitchSystemCode.AllOn,
                DipswitchUnitCode.B,
                RemoteSocketCommand.TurnOff);

            Assert.AreEqual(4436U, code.Value);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitCAndTurnOn()
        {
            var code = DipswitchCodeProvider.GetCode(
                    DipswitchSystemCode.Switch1 | DipswitchSystemCode.Switch3 | DipswitchSystemCode.Switch5,
                    DipswitchUnitCode.C,
                    RemoteSocketCommand.TurnOn);

            Assert.AreEqual(1119313U, code.Value);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitCAndTurnOff()
        {
            var code = DipswitchCodeProvider.GetCode(
                    DipswitchSystemCode.Switch1 | DipswitchSystemCode.Switch3 | DipswitchSystemCode.Switch5,
                    DipswitchUnitCode.C,
                    RemoteSocketCommand.TurnOff);

            Assert.AreEqual(1119316U, code.Value);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitDAndTurnOn()
        {
            var code = DipswitchCodeProvider.GetCode(
                    DipswitchSystemCode.AllOff,
                    DipswitchUnitCode.D,
                    RemoteSocketCommand.TurnOn);

            Assert.AreEqual(5592337U, code.Value);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitDAndTurnOff()
        {
            var code = DipswitchCodeProvider.GetCode(
                    DipswitchSystemCode.AllOff,
                    DipswitchUnitCode.D,
                    RemoteSocketCommand.TurnOff);

            Assert.AreEqual(5592340U, code.Value);
        }
    }
}
