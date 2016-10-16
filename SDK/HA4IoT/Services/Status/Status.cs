using System.Collections.Generic;

namespace HA4IoT.Services.Status
{
    public class Status
    {
        public List<WindowStatus> OpenWindows { get; } = new List<WindowStatus>();

        public List<WindowStatus> TiltWindows { get; } = new List<WindowStatus>();

        public List<ActuatorStatus> ActiveActuators { get; } = new List<ActuatorStatus>();
    }
}
