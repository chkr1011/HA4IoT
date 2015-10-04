using System.Linq;
using CK.HomeAutomation.Hardware.RemoteSwitch.Codes;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace CK.HomeAutomation.Hardware.RemoteSwitch.Tests
{
    [TestClass]
    public class BrennenstuhlCodeSequenceProviderTests
    {
        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitAAndTurnOn()
        {
            var generator = new BrennenstuhlCodeSequenceProvider();
            var sequence = generator.GetSequence(
                BrennenstuhlCodeSequenceProvider.SystemCode.AllOn,
                BrennenstuhlCodeSequenceProvider.UnitCode.A,
                RemoteSwitchCommand.TurnOn);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(1361UL);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitAAndTurnOff()
        {
            var generator = new BrennenstuhlCodeSequenceProvider();
            var sequence = generator.GetSequence(
                BrennenstuhlCodeSequenceProvider.SystemCode.AllOn,
                BrennenstuhlCodeSequenceProvider.UnitCode.A,
                RemoteSwitchCommand.TurnOff);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(1364UL);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitBAndTurnOn()
        {
            var generator = new BrennenstuhlCodeSequenceProvider();
            var sequence = generator.GetSequence(
                BrennenstuhlCodeSequenceProvider.SystemCode.AllOn,
                BrennenstuhlCodeSequenceProvider.UnitCode.B,
                RemoteSwitchCommand.TurnOn);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(4433UL);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitBAndTurnOff()
        {
            var generator = new BrennenstuhlCodeSequenceProvider();
            var sequence = generator.GetSequence(
                BrennenstuhlCodeSequenceProvider.SystemCode.AllOn,
                BrennenstuhlCodeSequenceProvider.UnitCode.B,
                RemoteSwitchCommand.TurnOff);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(4436UL);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitCAndTurnOn()
        {
            var generator = new BrennenstuhlCodeSequenceProvider();
            var sequence =
                generator.GetSequence(
                    BrennenstuhlCodeSequenceProvider.SystemCode.Switch1 | BrennenstuhlCodeSequenceProvider.SystemCode.Switch3 |
                    BrennenstuhlCodeSequenceProvider.SystemCode.Switch5,
                    BrennenstuhlCodeSequenceProvider.UnitCode.C,
                    RemoteSwitchCommand.TurnOn);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(1119313UL);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitCAndTurnOff()
        {
            var generator = new BrennenstuhlCodeSequenceProvider();
            var sequence =
                generator.GetSequence(
                    BrennenstuhlCodeSequenceProvider.SystemCode.Switch1 | BrennenstuhlCodeSequenceProvider.SystemCode.Switch3 |
                    BrennenstuhlCodeSequenceProvider.SystemCode.Switch5,
                    BrennenstuhlCodeSequenceProvider.UnitCode.C,
                    RemoteSwitchCommand.TurnOff);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(1119316UL);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitDAndTurnOn()
        {
            var generator = new BrennenstuhlCodeSequenceProvider();
            var sequence =
                generator.GetSequence(
                    BrennenstuhlCodeSequenceProvider.SystemCode.AllOff,
                    BrennenstuhlCodeSequenceProvider.UnitCode.D,
                    RemoteSwitchCommand.TurnOn);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(5592337UL);
        }

        [TestMethod]
        public void CodeShouldBeGeneratedCorrectly_WithUnitDAndTurnOff()
        {
            var generator = new BrennenstuhlCodeSequenceProvider();
            var sequence =
                generator.GetSequence(
                    BrennenstuhlCodeSequenceProvider.SystemCode.AllOff,
                    BrennenstuhlCodeSequenceProvider.UnitCode.D,
                    RemoteSwitchCommand.TurnOff);

            sequence.Codes.Count.ShouldBeEquivalentTo(1);
            sequence.Codes.First().Value.ShouldBeEquivalentTo(5592340UL);
        }
    }
}
