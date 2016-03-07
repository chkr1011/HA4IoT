using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Actuators
{
    public abstract class BinaryStateOutputActuatorBase<TSettings> : ActuatorBase<TSettings>, IBinaryStateOutputActuator where TSettings : ActuatorSettings
    {
        protected BinaryStateOutputActuatorBase(ActuatorId id, IHttpRequestController httpApiController, ILogger logger) 
            : base(id, httpApiController, logger)
        {
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

        public override void HandleApiPost(ApiRequestContext context)
        {
            base.HandleApiPost(context);

            if (!context.Request.ContainsKey("state"))
            {
                return;
            }

            string action = context.Request.GetNamedString("state", "toggle");
            bool commit = context.Request.GetNamedBoolean("commit", true);

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

                context.Response = ExportStatusToJsonObject();
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