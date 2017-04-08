using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Commands;
using HA4IoT.Components;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Exceptions;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using Newtonsoft.Json;
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
        private readonly ILogger _log;

        public ComponentRegistryService(
            ISystemEventsService systemEventsService,
            ISystemInformationService systemInformationService,
            IApiDispatcherService apiService,
            ISettingsService settingsService,
            ILogService logService)
        {
            if (systemEventsService == null) throw new ArgumentNullException(nameof(systemEventsService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _log = logService.CreatePublisher(nameof(ComponentRegistryService));

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
                    actuator.ExecuteCommand(new ResetCommand());
                }
                catch (Exception exception)
                {
                    _log.Warning(exception, $"Error while initially reset of state for actuator '{actuator.Id}'.");
                }
            }

            _systemInformationService.Set("Components/Count", _components.Count);
        }

        public void RegisterComponent(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            _components.Add(component.Id, component);

            component.StateChanged += (s, e) =>
            {
                var oldStateText = JToken.FromObject(e.OldState?.Serialize()).ToString(Formatting.None);
                var newStateText = JToken.FromObject(e.NewState?.Serialize()).ToString(Formatting.None);

                _log.Info($"Component '{((ComponentBase)s).Id}' updated state from:{oldStateText} to:{newStateText}");

                _apiService.NotifyStateChanged(component);
            };
        }

        public IComponent GetComponent(string id)
        {
            if (!ContainsComponent(id)) throw new ComponentNotFoundException(id);

            return _components[id];
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

            if (!ContainsComponent(id)) throw new ComponentNotFoundException(id);

            return (TComponent)_components[id];
        }

        [ApiMethod]
        public void ExecuteCommand(IApiContext apiContext)
        {
            var componentId = apiContext.Parameter["ComponentId"].Value<string>();
            var commandType = apiContext.Parameter["CommandType"].Value<string>();
            
            var commandResolver = new CommandResolver();
            ICommand command;
            try
            {
                command = commandResolver.Resolve(commandType, apiContext.Parameter);
            }
            catch (CommandUnknownException exception)
            {
                _log.Warning(exception, $"Tried to invoke unknown command '{commandType}'.");
                apiContext.ResultCode = ApiResultCode.InvalidParameter;
                return;
            }
            
            GetComponent(componentId).ExecuteCommand(command);
        }

        private void HandleApiStatusRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            var components = new JObject();
            foreach (var component in _components.Values.ToList())
            {
                var status = new JObject
                {
                    ["Settings"] = _settingsService.GetRawSettings(component),
                    ["State"] = JToken.FromObject(component.GetState().Serialize())
                };

                components[component.Id] = status;
            }

            e.ApiContext.Result["Components"] = components;
        }
    }
}
