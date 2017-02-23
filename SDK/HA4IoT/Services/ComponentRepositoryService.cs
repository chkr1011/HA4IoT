using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Services
{
    [ApiServiceClass(typeof(IComponentRegistryService))]
    public class ComponentRegistryService : ServiceBase, IComponentRegistryService
    {
        private readonly Dictionary<string, IComponent> _components = new Dictionary<string, IComponent>();

        private readonly ISystemInformationService _systemInformationService;
        private readonly IApiDispatcherService _apiService;
        private readonly ISettingsService _settingsService;

        public ComponentRegistryService(
            ISystemEventsService systemEventsService,
            ISystemInformationService systemInformationService,
            IApiDispatcherService apiService,
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
            foreach (var actuator in _components.Values)
            {
                try
                {
                    actuator.InvokeCommand(new ResetCommand());
                }
                catch (Exception exception)
                {
                    Log.Warning(exception, $"Error while initially reset of state for actuator '{actuator.Id}'.");
                }
            }

            _systemInformationService.Set("Components/Count", _components.Count);
        }

        public void AddComponent(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            _components.Add(component.Id, component);

            component.StateChanged += (s, e) => _apiService.NotifyStateChanged(component);
        }

        public IComponent GetComponent(string id)
        {
            return _components[id];
        }

        public TComponent GetComponent<TComponent>() where TComponent : IComponent
        {
            return _components.OfType<TComponent>().SingleOrDefault();
        }

        public IList<TComponent> GetComponents<TComponent>() where TComponent : IComponent
        {
            return _components.OfType<TComponent>().ToList();
        }

        public IList<IComponent> GetComponents()
        {
            return _components.Values.ToList();
        }

        public bool ContainsComponent(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            return _components.ContainsKey(id);
        }

        public TComponent GetComponent<TComponent>(string id) where TComponent : IComponent
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            return (TComponent)_components[id];
        }

        [ApiMethod]
        public void InvokeCommand(IApiContext apiContext)
        {
            //var component = _components.Get(new ComponentId(componentId));
            throw new NotImplementedException();
        }

        private void HandleApiStatusRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            var components = new JObject();
            foreach (var component in _components.Values)
            {
                var status = new JObject
                {
                    ["Settings"] = _settingsService.GetRawComponentSettings(component.Id),
                    ["State"] = JToken.FromObject(component.GetState().Serialize())
                };

                components[component.Id] = status;
            }

            e.Context.Result["Components"] = components;
        }
    }
}
