using System;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Services.Areas
{
    public static class AreaServiceExtensions
    {
        public static IArea CreateArea(this IAreaService areaService, Enum id)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            return areaService.CreateArea(AreaIdFactory.Create(id));
        }

        public static IArea GetArea(this IAreaService areaService, Enum id)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));

            return areaService.GetArea(AreaIdFactory.Create(id));
        }
    }
}
