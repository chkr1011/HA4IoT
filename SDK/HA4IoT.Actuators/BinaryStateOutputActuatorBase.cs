using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class BinaryStateOutputActuatorBase : ActuatorBase, IBinaryStateOutputActuator
    {
        protected BinaryStateOutputActuatorBase(string id, IHttpRequestController request,
            INotificationHandler log) : base(id, request, log)
        {
        }

        public event EventHandler<BinaryActuatorStateChangedEventArgs> StateChanged;

        public virtual void SetInitialState()
        {
            // TODO: Load state from file
            SetState(BinaryActuatorState.Off, new ForceUpdateStateParameter());
        }

        public BinaryActuatorState GetState()
        {
            return GetStateInternal();
        }

        public void SetState(BinaryActuatorState state, params IParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

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

                HandleApiGet(context);

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

        public override void HandleApiGet(ApiRequestContext context)
        {
            context.Response["state"] = JsonValue.CreateStringValue(GetStateInternal().ToString());
            base.HandleApiGet(context);
        }
        
        protected void OnStateChanged(BinaryActuatorState oldState, BinaryActuatorState newState)
        {
            StateChanged?.Invoke(this, new BinaryActuatorStateChangedEventArgs(oldState, newState));
        }

        protected abstract void SetStateInternal(BinaryActuatorState state, params IParameter[] parameters);

        protected abstract BinaryActuatorState GetStateInternal();
    }
}