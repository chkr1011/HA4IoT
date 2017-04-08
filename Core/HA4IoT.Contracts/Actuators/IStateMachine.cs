using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public interface IStateMachine : IComponent
    {
        string AlternativeStateId { get; set; }

        string ResetStateId { get; set; }

        bool SupportsState(string id);
    }
}
