using System;

namespace HA4IoT.Contracts.Core.Discovery
{
    public class DiscoveryResponse
    {
        public DiscoveryResponse(string controllerCaption, string controllerDescription)
        {
            if (controllerCaption == null) throw new ArgumentNullException(nameof(controllerCaption));
            if (controllerDescription == null) throw new ArgumentNullException(nameof(controllerDescription));

            ControllerCaption = controllerCaption;
            ControllerDescription = controllerDescription;
        }

        public string ControllerCaption { get; private set; }

        public string ControllerDescription { get; private set; }
    }
}
