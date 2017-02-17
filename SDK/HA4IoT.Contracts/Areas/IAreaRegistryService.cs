using System.Collections.Generic;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Areas
{
    public interface IAreaRegistryService : IService
    {
        IArea RegisterArea(string id);
        
        IArea GetArea(string id);

        IList<IArea> GetAreas();
    }
}
