using System;
using Windows.Data.Json;
using HA4IoT.Actuators.Actions;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Actuators
{
    public abstract class BinaryStateOutputActuatorBase<TSettings> : ActuatorBase<TSettings>, IBinaryStateOutputActuator where TSettings : ActuatorSettings
    {
        private readonly IActuatorAction _turnOnAction;
        private readonly IActuatorAction _turnOffAction;
        private readonly IActuatorAction _toggleAction;

        protected BinaryStateOutputActuatorBase(ActuatorId id, IApiController apiController, ILogger logger) 
            : base(id, apiController, logger)
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
        }

        public event EventHandler<BinaryActuatorStateChangedEventArgs> StateChanged;

        public BinaryActuatorState GetState()
        {
            return GetStateInternal();
        }

        public void SetState(BinaryActuatorState state, params IParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            if (!Settings.IsEnabled.Value)
            {
                return;
            }

            SetStateInternal(state, parameters);
        }

        public void TurnOff(params IParameter[] parameters)
        {
            SetState(BinaryActuatorState.Off, parameters);
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

        public override void HandleApiPost(IApiContext apiContext)
        {
            base.HandleApiPost(apiContext);

            if (!apiContext.Request.ContainsKey("state"))
            {
                return;
            }

            string action = apiContext.Request.GetNamedString("state", "toggle");
            bool commit = apiContext.Request.GetNamedBoolean("commit", true);

            if (action.Equals("toggle", StringComparison.OrdinalIgnoreCase))
            {
                if (commit)
                {
                    this.Toggle();
                }
                else
                {
                    this.Toggle(new DoNotCommitStateParameter());    
                }

                apiContext.Response = ExportStatusToJsonObject();
                return;
            }

            BinaryActuatorState state = (BinaryActuatorState)Enum.Parse(typeof (BinaryActuatorState), action, true);
            if (commit)
            {
                SetState(state);
            }
            else
            {
                SetState(state, new DoNotCommitStateParameter());
            }
        }

        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();
            status.SetNamedValue("state", JsonValue.CreateStringValue(GetStateInternal().ToString()));

            return status;
        }

        protected void OnStateChanged(BinaryActuatorState oldState, BinaryActuatorState newState)
        {
            StateChanged?.Invoke(this, new BinaryActuatorStateChangedEventArgs(oldState, newState));
        }

        protected abstract void SetStateInternal(BinaryActuatorState state, params IParameter[] parameters);

        protected abstract BinaryActuatorState GetStateInternal();
    }
}