using System;
using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Actuators.Contracts;

namespace CK.HomeAutomation.Tests.Mockups
{
    public class TestBinaryStateOutputActuator : IBinaryStateOutputActuator
    {
        public event EventHandler<BinaryActuatorStateChangedEventArgs> StateChanged;
        public string Id { get; set; }
        public BinaryActuatorState State { get; private set; }

        public void SetState(BinaryActuatorState state, bool commit = true)
        {
            if (state == State)
            {
                return;
            }

            State = state;

            var oldState = BinaryActuatorState.Off;
            if (state == BinaryActuatorState.Off)
            {
                oldState = BinaryActuatorState.On;
            }

            StateChanged?.Invoke(this, new BinaryActuatorStateChangedEventArgs(oldState, state));
        }

        public void Toggle(bool commit = true)
        {
            if (State == BinaryActuatorState.Off)
            {
                TurnOn();
            }
            else
            {
                TurnOff();
            }
        }

        public void TurnOff(bool commit = true)
        {
            SetState(BinaryActuatorState.Off);
        }

        public void TurnOn(bool commit = true)
        {
            SetState(BinaryActuatorState.On);
        }

        public TestBinaryStateOutputActuator WithOnState()
        {
            State = BinaryActuatorState.On;
            return this;
        }

        public TestBinaryStateOutputActuator WithOffState()
        {
            State = BinaryActuatorState.Off;
            return this;
        }
    }
}
