using System;
using System.Net;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Controller
{
    public class DiscoveryResponseReceivedEventArgs : EventArgs
    {
        public DiscoveryResponseReceivedEventArgs(IPEndPoint endPoint, JObject response)
        {
            if (endPoint == null) throw new ArgumentNullException(nameof(endPoint));
            if (response == null) throw new ArgumentNullException(nameof(response));

            EndPoint = endPoint;
            Response = response;
        }

        public IPEndPoint EndPoint { get; private set; }

        public JObject Response { get; private set; }
    }
}
