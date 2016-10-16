using System.Collections.Generic;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Areas
{
    public interface IAreaService : IService
    {
        IArea CreateArea(AreaId id);

        void AddArea(IArea area);

        IArea GetArea(AreaId id);

        IList<IArea> GetAreas();
    }
}
