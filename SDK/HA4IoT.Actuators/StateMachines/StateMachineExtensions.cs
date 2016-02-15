using System;
using HA4IoT.Contracts.Configuration;

namespace HA4IoT.Actuators
{
    public static class StateMachineExtensions
    {
        public static IArea WithStateMachine(this IArea room, Enum id, Action<StateMachine, IArea> initializer)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var stateMachine = new StateMachine(ActuatorIdFactory.Create(room, id), room.Controller.HttpApiController, room.Controller.Logger);
            initializer(stateMachine, room);
            stateMachine.SetInitialState();

            room.AddActuator(stateMachine);
            return room;
        }

        public static StateMachine StateMachine(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.Actuator<StateMachine>(ActuatorIdFactory.Create(room, id));
        }
    }
}
