using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Actuators
{
    public interface IFan : IComponent
    {
        IAction SetNextLevelAction { get; }
    }
}
