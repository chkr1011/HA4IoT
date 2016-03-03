namespace HA4IoT.Contracts.Actuators
{
    public interface IStateMachine : IActuator, IActuatorWithOffState
    {
        bool HasOffState { get; }

        void SetState(string id, params IParameter[] parameters);

        void SetNextState(params IParameter[] parameters);
    }
}
