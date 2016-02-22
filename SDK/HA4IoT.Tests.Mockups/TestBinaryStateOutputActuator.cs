using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Tests.Mockups
{
    public class TestBinaryStateOutputActuator : IBinaryStateOutputActuator
    {
        public event EventHandler<BinaryActuatorStateChangedEventArgs> StateChanged;

        public BinaryActuatorState State { get; private set; }

        public ActuatorId Id { get; set; }

        public IActuatorSettings Settings { get; }

        public JsonObject ExportConfigurationToJsonObject()
        {
            return new JsonObject();
        }

        public JsonObject ExportStatusToJsonObject()
        {
            return new JsonObject();
        }

        public void LoadSettings()
        {
        }

        public void ExposeToApi()
        {
            
        }

        public BinaryActuatorState GetState()
        {
            return State;
        }

        public void SetInitialState()
        {
            SetState(BinaryActuatorState.Off);
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

        public void TurnOff(params IParameter[] parameters)
        {
            State = BinaryActuatorState.Off;
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