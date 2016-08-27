using System;
using System.Collections.Generic;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking.Http;
using HA4IoT.Services.System;
using HA4IoT.Settings;

namespace HA4IoT.Services.Components
{
    [ApiServiceClass(typeof(IComponentService))]
    public class ComponentService : ServiceBase, IComponentService
    {
        private readonly ISystemInformationService _systemInformationService;
        private readonly IApiService _apiService;
        private readonly ComponentCollection _components = new ComponentCollection();

        public ComponentService(
            ISystemEventsService systemEventsService,
            ISystemInformationService systemInformationService,
            IApiService apiService)
        {
            if (systemEventsService == null) throw new ArgumentNullException(nameof(systemEventsService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));

            _systemInformationService = systemInformationService;
            _apiService = apiService;

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

            _apiService.Route($"component/{component.Id}/status", component.HandleApiCall);
            new SettingsContainerApiDispatcher(component.Settings, $"component/{component.Id}", _apiService).ExposeToApi();
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

        [ApiMethod(ApiCallType.Command)]
        public void Update(IApiContext apiContext)
        {
            // TODO: Consider creating classes as optional method parameters which are filled via reflection from Request JSON.
            var componentId = apiContext.Request.GetNamedString("ComponentId", string.Empty);
            if (string.IsNullOrEmpty(componentId))
            {
                throw new BadRequestException("Property 'ComponentId' is missing.");
            }

            var component = _components.Get(new ComponentId(componentId));
            component.HandleApiCall(apiContext);
        }

        [ApiMethod(ApiCallType.Command)]
        public void Status(IApiContext apiContext)
        {
            
        }

        [ApiMethod(ApiCallType.Command)]
        public void Reset(IApiContext apiContext)
        {
            var componentId = apiContext.Request.GetNamedString("ComponentId", string.Empty);
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
            var components = new JsonObject();
            foreach (var component in _components.GetAll())
            {
                components.SetNamedValue(component.Id.Value, component.ExportStatusToJsonObject());
            }

            e.Context.Response.SetNamedValue("components", components);
        }
    }
}
