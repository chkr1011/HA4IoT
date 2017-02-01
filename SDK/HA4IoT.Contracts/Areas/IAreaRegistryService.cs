using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Areas
{
    public interface IAreaRegistryService : IService
    {
        IArea RegisterArea(AreaId id);
        
        IArea GetArea(AreaId id);

        IList<IArea> GetAreas();
    }
}
