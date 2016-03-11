using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Contracts.Core
{
    public interface IController : IDeviceController, IActuatorController, IAreaController, IAutomationController
    {
        ILogger Logger { get; }
        IApiController ApiController { get; }
        IHomeAutomationTimer Timer { get; }    
        IControllerSettings Settings { get; }  
    }
}
