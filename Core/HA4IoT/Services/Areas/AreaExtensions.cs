using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Services.Areas
{
    public static class AreaExtensions
    {
        public static IComponent GetComponent(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent(area.Id + "." + id);
        }
    }
}
