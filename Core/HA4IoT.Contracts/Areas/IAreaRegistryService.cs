using HA4IoT.Contracts.Services;
using System.Collections.Generic;

namespace HA4IoT.Contracts.Areas
{
    public interface IAreaRegistryService : IService
    {
        IArea RegisterArea(string id);
        
        IArea GetArea(string id);

        IList<IArea> GetAreas();
    }
}
