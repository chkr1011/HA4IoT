using System.Collections.Generic;

namespace HA4IoT.Contracts.Actuators
{
    public interface IWindow : IActuator
    {
        IList<ICasement> Casements { get; }
    }
}