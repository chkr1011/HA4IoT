using System.Linq;
using FluentAssertions;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Hardware.RemoteSwitch.Codes;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Hardware.Tests.RemoteSwitch
{
    [TestClass]
    public class BrennenstuhlCodeSequenceProviderTests
    {
        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitAAndTurnOn()
        {
            var generator = new BrennenstuhlCodeSequenceProvider();
            var sequence = generator.GetSequence(
                BrennenstuhlSystemCode.AllOn,
                BrennenstuhlUnitCode.A,
                RemoteSocketCommand.TurnOn);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(1361U);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitAAndTurnOff()
        {
            var generator = new BrennenstuhlCodeSequenceProvider();
            var sequence = generator.GetSequence(
                BrennenstuhlSystemCode.AllOn,
                BrennenstuhlUnitCode.A,
                RemoteSocketCommand.TurnOff);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(1364U);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitBAndTurnOn()
        {
            var generator = new BrennenstuhlCodeSequenceProvider();
            var sequence = generator.GetSequence(
                BrennenstuhlSystemCode.AllOn,
                BrennenstuhlUnitCode.B,
                RemoteSocketCommand.TurnOn);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(4433U);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitBAndTurnOff()
        {
            var generator = new BrennenstuhlCodeSequenceProvider();
            var sequence = generator.GetSequence(
                BrennenstuhlSystemCode.AllOn,
                BrennenstuhlUnitCode.B,
                RemoteSocketCommand.TurnOff);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(4436U);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitCAndTurnOn()
        {
            var generator = new BrennenstuhlCodeSequenceProvider();
            var sequence =
                generator.GetSequence(
                    BrennenstuhlSystemCode.Switch1 | BrennenstuhlSystemCode.Switch3 | BrennenstuhlSystemCode.Switch5,
                    BrennenstuhlUnitCode.C,
                    RemoteSocketCommand.TurnOn);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(1119313U);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitCAndTurnOff()
        {
            var generator = new BrennenstuhlCodeSequenceProvider();
            var sequence =
                generator.GetSequence(
                    BrennenstuhlSystemCode.Switch1 | BrennenstuhlSystemCode.Switch3 | BrennenstuhlSystemCode.Switch5,
                    BrennenstuhlUnitCode.C,
                    RemoteSocketCommand.TurnOff);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(1119316U);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitDAndTurnOn()
        {
            var generator = new BrennenstuhlCodeSequenceProvider();
            var sequence =
                generator.GetSequence(
                    BrennenstuhlSystemCode.AllOff,
                    BrennenstuhlUnitCode.D,
                    RemoteSocketCommand.TurnOn);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(5592337U);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitDAndTurnOff()
        {
            var generator = new BrennenstuhlCodeSequenceProvider();
            var sequence =
                generator.GetSequence(
                    BrennenstuhlSystemCode.AllOff,
                    BrennenstuhlUnitCode.D,
                    RemoteSocketCommand.TurnOff);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(5592340U);
        }
    }
}
