using System;
using Windows.Data.Json;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Tests.Mockups
{
    public class TestRollerShutter : IRollerShutter
    {
        private RollerShutterState _state = RollerShutterState.Stopped;

        public TestRollerShutter(ActuatorId id, ILogger logger)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            Id = id;
            Settings = new RollerShutterSettings(id, logger);
        }

        public event EventHandler<RollerShutterStateChangedEventArgs> StateChanged;
        public ActuatorId Id { get; }
        public IRollerShutterSettings Settings { get; }
        public bool IsClosed { get; set; }

        public JsonObject ExportConfigurationToJsonObject()
        {
            throw new NotSupportedException();
        }

        public JsonObject ExportStatusToJsonObject()
        {
            throw new NotSupportedException();
        }

        public void LoadSettings()
        {
        }

        public void ExposeToApi()
        {
        }

        public void TurnOff(params IParameter[] parameters)
        {
            SetState(RollerShutterState.Stopped);
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
