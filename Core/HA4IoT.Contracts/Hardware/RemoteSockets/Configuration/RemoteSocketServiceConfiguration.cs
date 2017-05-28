using System.Collections.Generic;

namespace HA4IoT.Contracts.Hardware.RemoteSockets.Configuration
{
    public class RemoteSocketServiceConfiguration
    {
        public Dictionary<string, RemoteSocketConfiguration> RemoteSockets = new Dictionary<string, RemoteSocketConfiguration>();
    }
}
