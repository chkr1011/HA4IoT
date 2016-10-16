using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking.Http;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Services.Components
{
    [ApiServiceClass(typeof(IComponentService))]
    public class ComponentService : ServiceBase, IComponentService
    {
        private readonly ComponentCollection _components = new ComponentCollection();

        private readonly ISystemInformationService _systemInformationService;
        private readonly IApiService _apiService;
        private readonly ISettingsService _settingsService;

        public ComponentService(
            ISystemEventsService systemEventsService,
            ISystemInformationService systemInformationService,
            IApiService apiService,
            ISettingsService settingsService)
        {
            if (systemEventsService == null) throw new ArgumentNullException(nameof(systemEventsService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _systemInformationService = systemInformationService;
            _apiService = apiService;
            _settingsService = settingsService;

            apiService.StatusRequested += HandleApiStatusRequest;
        }

        public override void Startup()
        {
            foreach (var actuator in _components.GetAll<IActuator>())
            {
                try
                {
                    actuator.ResetState();
                }
                catch (Exception exception)
                {
                    Log.Warning(exception, $"Error while initially reset of state for actuator '{actuator.Id}'.");
                }
            }

            _systemInformationService.Set("Components/Count", _components.GetAll().Count);
        }

        public void AddComponent(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            _components.AddUnique(component.Id, component);

            component.StateChanged += (s, e) => _apiService.NotifyStateChanged(component);
        }

        public TComponent GetComponent<TComponent>() where TComponent : IComponent
        {
            return _components.Get<TComponent>();
        }

        public IList<TComponent> GetComponents<TComponent>() where TComponent : IComponent
        {
            return _components.GetAll<TComponent>();
        }

        public IList<IComponent> GetComponents()
        {
            return _components.GetAll();
        }

        public bool ContainsComponent(ComponentId componentId)
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));

            return _components.Contains(componentId);
        }

        public TComponent GetComponent<TComponent>(ComponentId id) where TComponent : IComponent
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            return _components.Get<TComponent>(id);
        }

        [ApiMethod]
        public void Invoke(IApiContext apiContext)
        {
            // TODO: Consider creating classes as optional method parameters which are filled via reflection from Request JSON.
            var componentId = (string)apiContext.Request["ComponentId"];
            if (string.IsNullOrEmpty(componentId))
            {
                throw new BadRequestException("Property 'ComponentId' is missing.");
            }

            var component = _components.Get(new ComponentId(componentId));
            component.HandleApiCall(apiContext);
        }

        [ApiMethod]
        public void Status(IApiContext apiContext)
        {
            
        }

        [ApiMethod]
        public void Reset(IApiContext apiContext)
        {
            var componentId = (string)apiContext.Request["ComponentId"];
            if (string.IsNullOrEmpty(componentId))
            {
                throw new BadRequestException("Property 'ComponentId' is missing.");
            }

            var component = _components.Get(new ComponentId(componentId));
            var actuator = component as IActuator;
            if (actuator == null)
            {
                throw new BadRequestException("The component is no actuator.");
            }

            actuator.ResetState();
        }

        private void HandleApiStatusRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            var components = new JObject();
            foreach (var component in _components.GetAll())
            {
                var status = component.ExportStatus();
                status["Settings"] = _settingsService.GetRawSettings(component.Id);

                components[component.Id.Value] = status;
            }

            e.Context.Response["Components"] = components;
        }
    }
}
