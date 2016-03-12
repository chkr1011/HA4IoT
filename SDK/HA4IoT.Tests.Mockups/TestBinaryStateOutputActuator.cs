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
        private readonly IHomeAutomationAction _turnOnAction;
        private readonly IHomeAutomationAction _turnOffAction;
        private readonly IHomeAutomationAction _toggleAction;

        public TestBinaryStateOutputActuator()
        {
            _turnOnAction = new HomeAutomationAction(() => SetState(BinaryActuatorState.On));
            _turnOffAction = new HomeAutomationAction(() => SetState(BinaryActuatorState.Off));
            _toggleAction = new HomeAutomationAction(() =>
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

        public void SetState(BinaryActuatorState state, params IHardwareParameter[] parameters)
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

        public void ToggleState(params IHardwareParameter[] parameters)
        {
        }

        public IHomeAutomationAction GetTurnOnAction()
        {
            return _turnOnAction;
        }

        public IHomeAutomationAction GetTurnOffAction()
        {
            return _turnOffAction;
        }

        public IHomeAutomationAction GetToggleStateAction()
        {
            return _toggleAction;
        }

        public void TurnOff(params IHardwareParameter[] parameters)
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