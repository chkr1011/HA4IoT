using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Contracts.Core
{
    public interface IController : IDeviceController, IComponentController, IAreaController, IAutomationController
    {
        IApiController ApiController { get; }
        IHomeAutomationTimer Timer { get; }    
        ISettingsContainer Settings { get; }  
        IServiceLocator ServiceLocator { get; }
    }
}
