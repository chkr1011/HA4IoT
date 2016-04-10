using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Core
{
    public static class ControllerExtensions
    {
        public static IArea CreateArea(this IController controller, Enum id)
        {
            var area = new Area(AreaIdFactory.CreateIdFrom(id), controller);
            controller.AddArea(area);

            return area;
        }

        public static IArea GetArea(this IController controller, Enum id)
        {
            return controller.GetArea(AreaIdFactory.CreateIdFrom(id));
        }
    }
}
