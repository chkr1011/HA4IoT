using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Tests.Mockups
{
    public class TestStateMachine : IStateMachine
    {
        private StateMachineStateId _activeState;

        public TestStateMachine()
        {
            _activeState = DefaultStateIDs.Off;
        }

        public TestStateMachine(StateMachineStateId initialState)
        {
            if (initialState == null) throw new ArgumentNullException(nameof(initialState));

            _activeState = initialState;
        }

        public ActuatorId Id { get; set; }
        public ISettingsContainer Settings { get; }
        public IActuatorSettingsWrapper GeneralSettingsWrapper { get; }

        public JsonObject ExportConfigurationToJsonObject()
        {
            throw new NotImplementedException();
        }

        public JsonObject ExportStatusToJsonObject()
        {
            throw new NotImplementedException();
        }

        public void ExposeToApi(IApiController apiController)
        {
        }

        public event EventHandler<StateMachineStateChangedEventArgs> ActiveStateChanged;

        public bool GetSupportsState(StateMachineStateId stateId)
        {
            return true;
        }

        public StateMachineStateId GetActiveState()
        {
            return _activeState;
        }

        public void SetActiveState(StateMachineStateId id, params IHardwareParameter[] parameters)
        {
            var oldState = _activeState;

            _activeState = id;
            ActiveStateChanged?.Invoke(this, new StateMachineStateChangedEventArgs(oldState, id));
        }

        public StateMachineStateId GetNextState(StateMachineStateId stateId)
        {
            if (stateId.Equals(DefaultStateIDs.On))
            {
                return DefaultStateIDs.Off;
            }

            return DefaultStateIDs.On;
        }

        public void SetStateIdAlias(StateMachineStateId stateId, StateMachineStateId alias)
        {
        }
    }
}
