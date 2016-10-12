using System.Collections.Generic;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public interface IWindow : ISensor
    {
        IList<ICasement> Casements { get; }

        IComponentSettings Settings { get; }
    }
}