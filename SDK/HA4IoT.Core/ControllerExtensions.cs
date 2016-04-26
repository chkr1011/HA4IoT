using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Core
{
    public static class ControllerExtensions
    {
        public static IArea CreateArea(this IController controller, Enum id)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            var area = new Area(AreaIdFactory.Create(id), controller);
            controller.AddArea(area);

            return area;
        }

        public static IArea GetArea(this IController controller, Enum id)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            return controller.GetArea(AreaIdFactory.Create(id));
        }
    }
}
