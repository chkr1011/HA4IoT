using System;
using Windows.Data.Json;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Actions;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Tests.Mockups
{
    public class TestBinaryStateOutputActuator : IBinaryStateOutputActuator
    {
        private readonly IActuatorAction _turnOnAction;
        private readonly IActuatorAction _turnOffAction;
        private readonly IActuatorAction _toggleAction;

        public TestBinaryStateOutputActuator()
        {
            _turnOnAction = new ActuatorAction(() => SetState(BinaryActuatorState.On));
            _turnOffAction = new ActuatorAction(() => SetState(BinaryActuatorState.Off));
            _toggleAction = new ActuatorAction(() =>
            {
                if (GetState() == BinaryActuatorState.On)
                {
                    SetState(BinaryActuatorState.Off);
                }
                else if (GetState() == BinaryActuatorState.Off)
                {
                    SetState(BinaryActuatorState.On);
                }
            });

            Settings = new ActuatorSettings(ActuatorIdFactory.EmptyId, new TestLogger());
        }

        public event EventHandler<BinaryActuatorStateChangedEventArgs> StateChanged;

        public ActuatorId Id { get; }
        public IActuatorSettings Settings { get; }
        public BinaryActuatorState State { get; private set; }

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

        public IActuatorAction GetTurnOnAction()
        {
            return _turnOnAction;
        }

        public IActuatorAction GetTurnOffAction()
        {
            return _turnOffAction;
        }

        public IActuatorAction GetToggleAction()
        {
            return _toggleAction;
        }

        public void TurnOff(params IParameter[] parameters)
        {
            State = BinaryActuatorState.Off;
        }

        public void SetInitialState()
        {
            SetState(BinaryActuatorState.Off);
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