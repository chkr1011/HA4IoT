using System;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Tests.Mockups
{
    public class TestBinaryStateOutputActuator : IBinaryStateOutputActuator
    {
        public event EventHandler<BinaryActuatorStateChangedEventArgs> StateChanged;
        public string Id { get; set; }
        public BinaryActuatorState State { get; private set; }

        public BinaryActuatorState GetState()
        {
            return State;
        }

        public void SetState(BinaryActuatorState state, params IParameter[] parameters)
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

        public void TurnOff(params IParameter[] parameters)
        {
            State = BinaryActuatorState.Off;
        }
    }
}
