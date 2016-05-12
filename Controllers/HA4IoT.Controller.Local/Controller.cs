using Windows.Devices.Sensors;
using HA4IoT.Contracts.Areas;
using HA4IoT.Core;

namespace HA4IoT.Controller.Local
{
    public class Controller : ControllerBase
    {
        protected override void Initialize()
        {
            var area = new Area(new AreaId("TestArea"), this);

            AddArea(area);
        }
    }
}
