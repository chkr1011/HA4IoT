using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Networking;
using HA4IoT.Notifications;

namespace HA4IoT.Actuators
{
    public abstract class BinaryStateOutputActuatorBase : ActuatorBase, IBinaryStateOutputActuator
    {
        protected BinaryStateOutputActuatorBase(string id, IHttpRequestController httpRequestController,
            INotificationHandler notificationHandler) : base(id, httpRequestController, notificationHandler)
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

            SetStateInternal(state, parameters);
        }

        public override void ApiPost(ApiRequestContext context)
        {
            base.ApiPost(context);

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

                ApiGet(context);

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

        public override void ApiGet(ApiRequestContext context)
        {
            bool isOn = GetStateInternal() == BinaryActuatorState.On;

            context.Response["state"] = JsonValue.CreateStringValue(GetStateInternal().ToString());
            context.Response["stateBool"] = JsonValue.CreateBooleanValue(isOn);
            base.ApiGet(context);
        }
        
        protected void OnStateChanged(BinaryActuatorState oldState, BinaryActuatorState newState)
        {
            StateChanged?.Invoke(this, new BinaryActuatorStateChangedEventArgs(oldState, newState));
        }

        protected abstract void SetStateInternal(BinaryActuatorState state, params IParameter[] parameters);

        protected abstract BinaryActuatorState GetStateInternal();
    }
}