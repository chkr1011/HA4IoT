using System.Collections.Generic;

namespace HA4IoT.Services.Status
{
    public class Status
    {
        public List<WindowStatus> OpenWindows { get; } = new List<WindowStatus>();

        public List<WindowStatus> TiltWindows { get; } = new List<WindowStatus>();

        public List<ComponentStatus> ActiveComponents { get; } = new List<ComponentStatus>();
    }
}
