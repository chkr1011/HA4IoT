using System.Collections.Generic;

namespace HA4IoT.Contracts.Sensors
{
    public interface IWindow : ISensor
    {
        IList<ICasement> Casements { get; }
    }
}