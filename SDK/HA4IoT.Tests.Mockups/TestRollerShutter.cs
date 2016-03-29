using System;
using Windows.Data.Json;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Core.Settings;

namespace HA4IoT.Tests.Mockups
{
    public class TestRollerShutter : IRollerShutter
    {
        private RollerShutterState _state = RollerShutterState.Stopped;

        public TestRollerShutter(ActuatorId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;

            Settings = new SettingsContainer(string.Empty);
            GeneralSettingsWrapper = new RollerShutterSettingsWrapper(Settings);
        }

        public event EventHandler<RollerShutterStateChangedEventArgs> StateChanged;
        public ActuatorId Id { get; }
        public ISettingsContainer Settings { get; }
        public IActuatorSettingsWrapper GeneralSettingsWrapper { get; }
        public bool IsClosed { get; set; }

        public JsonObject ExportConfigurationToJsonObject()
        {
            throw new NotSupportedException();
        }

        public JsonObject ExportStatusToJsonObject()
        {
            throw new NotSupportedException();
        }

        public void ExposeToApi(IApiController apiController)
        {
        }

        public void TurnOff(params IHardwareParameter[] parameters)
        {
            SetState(RollerShutterState.Stopped);
        }

        public IHomeAutomationAction GetTurnOffAction()
        {
            return null;
        }

        public RollerShutterState GetState()
        {
            return _state;
        }

        public void SetState(RollerShutterState state)
        {
            var oldState = GetState();
            _state = state;

            StateChanged?.Invoke(this, new RollerShutterStateChangedEventArgs(oldState, _state));
        }
    }
}
