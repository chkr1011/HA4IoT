using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Automations;

namespace HA4IoT.Contracts.Core
{
    public interface IController : IDeviceController, IActuatorController, IAreaController, IAutomationController, IServiceController
    {
        IApiController ApiController { get; }
        IHomeAutomationTimer Timer { get; }    
        IControllerSettings Settings { get; }  
    }
}
