using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public class StateMachine : ActuatorBase
    {
        private int _index;
        private bool _turnOffIfStateIsAppliedTwice;

        public StateMachine(string id, IHttpRequestController httpApiController, INotificationHandler notificationHandler)
            : base(id, httpApiController, notificationHandler)
        {
        }

        public string State => States[_index].Id;

        public List<StateMachineState> States { get; } = new List<StateMachineState>();

        public bool HasOffState
        {
            get { return States.Any(s => s.Id.Equals(BinaryActuatorState.Off.ToString(), StringComparison.OrdinalIgnoreCase)); }
        }

        public event EventHandler<StateMachineStateChangedEventArgs> StateChanged;

        public StateMachineState AddOffState()
        {
            return AddState(BinaryActuatorState.Off.ToString());
        }

        public StateMachineState AddOnState()
        {
            return AddState(BinaryActuatorState.On.ToString());
        }

        public StateMachineState AddState(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var state = new StateMachineState(id, this);
            States.Add(state);
            return state;
        }

        public StateMachineState AddState()
        {
            string id = (States.Count + 1).ToString();
            return AddState(id);
        }

        public void ApplyState(string id, bool commit = true)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            string oldState = State;

            for (int i = 0; i < States.Count; i++)
            {
                var state = States[i];

                if (state.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                {
                    bool stateIsAlreadyActive = i == _index;
                    if (stateIsAlreadyActive && _turnOffIfStateIsAppliedTwice)
                    {
                        if (state.Id == BinaryActuatorState.Off.ToString())
                        {
                            // The state is already "Off".
                            return;
                        }

                        TurnOff();
                        return;
                    }

                    _index = i;
                    state.Apply(commit);
                    StateChanged?.Invoke(this, new StateMachineStateChangedEventArgs(oldState, state.Id));
                    return;
                }
            }

            throw new NotSupportedException("StateMachineActuator '" + Id + "' does not support state '" + id + "'.");
        }

        public void ApplyNextState(bool commit = true)
        {
            _index += 1;
            if (_index >= States.Count)
            {
                _index = 0;
            }

            string oldState = State;
            string newState = States[_index].Id;

            States[_index].Apply(commit);
            StateChanged?.Invoke(this, new StateMachineStateChangedEventArgs(oldState, newState));
        }

        public void TurnOff(bool commit = true)
        {
            ApplyState(BinaryActuatorState.Off.ToString(), commit);
        }

        public StateMachine WithTurnOffIfStateIsAppliedTwice()
        {
            _turnOffIfStateIsAppliedTwice = true;
            return this;
        }

        public override void ApiGet(ApiRequestContext context)
        {
            if (!States.Any())
            {
                return;
            }

            context.Response.SetNamedValue("state", JsonValue.CreateStringValue(States[_index].Id));
        }

        public override JsonObject ApiGetConfiguration()
        {
            JsonObject configuration = base.ApiGetConfiguration();

            JsonArray stateMachineStates = new JsonArray();
            foreach (var state in States)
            {
                stateMachineStates.Add(JsonValue.CreateStringValue(state.Id));
            }

            configuration.SetNamedValue("states", stateMachineStates);

            return configuration;
        }

        public override void ApiPost(ApiRequestContext context)
        {
            if (!context.Request.ContainsKey("state"))
            {
                return;
            }

            string stateId = context.Request.GetNamedString("state", string.Empty);
            ApplyState(stateId);
        }
    }
}