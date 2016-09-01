using System.Collections.Generic;

namespace HA4IoT.Contracts.Areas
{
    public interface IAreaService
    {
        IArea CreateArea(AreaId id);

        void AddArea(IArea area);

        IArea GetArea(AreaId id);

        IList<IArea> GetAreas();
    }
}
