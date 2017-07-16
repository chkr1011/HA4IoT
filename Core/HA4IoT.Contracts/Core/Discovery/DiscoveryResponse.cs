using System;

namespace HA4IoT.Contracts.Core.Discovery
{
    public class DiscoveryResponse
    {
        public DiscoveryResponse(string controllerCaption, string controllerDescription)
        {
            ControllerCaption = controllerCaption ?? throw new ArgumentNullException(nameof(controllerCaption));
            ControllerDescription = controllerDescription ?? throw new ArgumentNullException(nameof(controllerDescription));
        }

        public string ControllerCaption { get; }

        public string ControllerDescription { get; }
    }
}
