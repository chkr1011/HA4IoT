using System.Linq;
using FluentAssertions;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Hardware.RemoteSwitch.Codes;
using HA4IoT.Hardware.RemoteSwitch.Codes.Protocols;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Hardware.RemoteSwitch
{
    [TestClass]
    public class BrennenstuhlCodeSequenceProviderTests
    {
        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitAAndTurnOn()
        {
            var generator = new DipswitchCodeSequenceProvider();
            var sequence = generator.GetSequence(
                DipswitchSystemCode.AllOn,
                DipswitchUnitCode.A,
                RemoteSocketCommand.TurnOn);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(1361U);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitAAndTurnOff()
        {
            var generator = new DipswitchCodeSequenceProvider();
            var sequence = generator.GetSequence(
                DipswitchSystemCode.AllOn,
                DipswitchUnitCode.A,
                RemoteSocketCommand.TurnOff);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(1364U);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitBAndTurnOn()
        {
            var generator = new DipswitchCodeSequenceProvider();
            var sequence = generator.GetSequence(
                DipswitchSystemCode.AllOn,
                DipswitchUnitCode.B,
                RemoteSocketCommand.TurnOn);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(4433U);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitBAndTurnOff()
        {
            var generator = new DipswitchCodeSequenceProvider();
            var sequence = generator.GetSequence(
                DipswitchSystemCode.AllOn,
                DipswitchUnitCode.B,
                RemoteSocketCommand.TurnOff);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(4436U);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitCAndTurnOn()
        {
            var generator = new DipswitchCodeSequenceProvider();
            var sequence =
                generator.GetSequence(
                    DipswitchSystemCode.Switch1 | DipswitchSystemCode.Switch3 | DipswitchSystemCode.Switch5,
                    DipswitchUnitCode.C,
                    RemoteSocketCommand.TurnOn);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(1119313U);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitCAndTurnOff()
        {
            var generator = new DipswitchCodeSequenceProvider();
            var sequence =
                generator.GetSequence(
                    DipswitchSystemCode.Switch1 | DipswitchSystemCode.Switch3 | DipswitchSystemCode.Switch5,
                    DipswitchUnitCode.C,
                    RemoteSocketCommand.TurnOff);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(1119316U);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitDAndTurnOn()
        {
            var generator = new DipswitchCodeSequenceProvider();
            var sequence =
                generator.GetSequence(
                    DipswitchSystemCode.AllOff,
                    DipswitchUnitCode.D,
                    RemoteSocketCommand.TurnOn);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(5592337U);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitDAndTurnOff()
        {
            var generator = new DipswitchCodeSequenceProvider();
            var sequence =
                generator.GetSequence(
                    DipswitchSystemCode.AllOff,
                    DipswitchUnitCode.D,
                    RemoteSocketCommand.TurnOff);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(5592340U);
        }
    }
}
