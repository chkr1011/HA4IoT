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
        private readonly object _syncRoot = new object();

        private readonly IHomeAutomationAction _turnOnAction;
        private readonly IHomeAutomationAction _turnOffAction;
        private readonly IHomeAutomationAction _toggleAction;

        protected BinaryStateOutputActuatorBase(ActuatorId id, IApiController apiController, ILogger logger) 
            : base(id, apiController, logger)
        {
            _turnOnAction = new HomeAutomationAction(() => SetState(BinaryActuatorState.On));
            _turnOffAction = new HomeAutomationAction(() => SetState(BinaryActuatorState.Off));
            _toggleAction = new HomeAutomationAction(() => ToggleState());
        }

        public event EventHandler<BinaryActuatorStateChangedEventArgs> StateChanged;

        public BinaryActuatorState GetState()
        {
            lock (_syncRoot)
            {
                return GetStateInternal();
            }
        }

        public void SetState(BinaryActuatorState state, params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            if (!Settings.IsEnabled.Value)
            {
                return;
            }

            lock (_syncRoot)
            {
                SetStateInternal(state, parameters);
            }
        }

        public void TurnOff(params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            SetState(BinaryActuatorState.Off, parameters);
        }

        public void ToggleState(params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            lock (_syncRoot)
            {
                if (GetState() == BinaryActuatorState.Off)
                {
                    SetState(BinaryActuatorState.On, parameters);
                }
                else
                {
                    SetState(BinaryActuatorState.Off, parameters);
                }
            }
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

        protected override void HandleApiCommand(IApiContext apiContext)
        {
            base.HandleApiCommand(apiContext);

            if (!apiContext.Request.ContainsKey("state"))
            {
                apiContext.ResultCode = ApiResultCode.InvalidBody;
                return;
            }

            string action = apiContext.Request.GetNamedString("state", "toggle");
 
            if (action.Equals("toggle", StringComparison.OrdinalIgnoreCase))
            {
                ToggleState();

                apiContext.Response = ExportStatusToJsonObject();
                return;
            }

            BinaryActuatorState state = (BinaryActuatorState)Enum.Parse(typeof (BinaryActuatorState), action, true);
            SetState(state);
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

        protected abstract void SetStateInternal(BinaryActuatorState state, params IHardwareParameter[] parameters);

        protected abstract BinaryActuatorState GetStateInternal();
    }
}