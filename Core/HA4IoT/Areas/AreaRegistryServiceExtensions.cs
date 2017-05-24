using System;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Areas
{
    public static class AreaRegistryServiceExtensions
    {
        public static IArea RegisterArea(this IAreaRegistryService areaService, Enum id)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            return areaService.RegisterArea(id.ToString());
        }

        public static IArea GetArea(this IAreaRegistryService areaService, Enum id)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));

            return areaService.GetArea(id.ToString());
        }
    }
}
