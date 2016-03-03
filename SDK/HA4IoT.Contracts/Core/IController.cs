using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Contracts.Core
{
    public interface IController : IDeviceController, IActuatorController, IAreaController, IAutomationController
    {
        ILogger Logger { get; }
        IHttpRequestController HttpApiController { get; }
        IHomeAutomationTimer Timer { get; }    
        IControllerSettings Settings { get; }  
    }
}
