using System.Collections.Generic;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Contracts.Sensors
{
    public interface IWindow : IActuator
    {
        IList<ICasement> Casements { get; }
    }
}