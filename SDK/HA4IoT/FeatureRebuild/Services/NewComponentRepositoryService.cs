using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services;
using HA4IoT.FeatureRebuild.Api;
using HA4IoT.FeatureRebuild.Commands;
using HA4IoT.FeatureRebuild.Status;
using Newtonsoft.Json.Linq;

namespace HA4IoT.FeatureRebuild.Services
{
    [ApiServiceClass(typeof(NewComponentRepositoryService))]
    public class NewComponentRepositoryService : ServiceBase
    {
        private readonly Dictionary<ComponentId, Component> _components = new Dictionary<ComponentId, Component>();

        public void Register(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            lock (_components)
            {
                _components.Add(component.Id, component);
            }
        }

        public void InvokeCommand(ComponentId componentId, ICommand command)
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));
            if (command == null) throw new ArgumentNullException(nameof(command));

            lock (_components)
            {
                _components[componentId].InvokeCommand(command);
            }
        }

        [ApiMethod]
        public void InvokeCommand(IApiContext apiContext)
        {
            var request = apiContext.Parameter.ToObject<InvokeCommandRequest>();
            var componentId = new ComponentId(request.ComponentId);
            var commandType = Type.GetType(request.Command);
            var command = request.CommandParameters.ToObject(commandType);

            InvokeCommand(componentId, (ICommand)command);
        }

        [ApiMethod]
        public void GetStatus(IApiContext apiContext)
        {
            var response = new Dictionary<ComponentId, Dictionary<string, IStatus>>();
            lock (_components)
            {
                foreach (var component in _components)
                {
                    var status = component.Value.GetStatus().ToDictionary(i => i.GetType().Name, i => i);
                    response.Add(component.Key, status);
                }
            }

            apiContext.Result = JObject.FromObject(response);
        }

        [ApiMethod]
        public void GetComponents(IApiContext apiContext)
        {
            var response = new Dictionary<ComponentId, ComponentConfiguration>();
            lock (_components)
            {
                foreach (var component in _components)
                {
                    var componentConfiguration = new ComponentConfiguration
                    {
                        Settings = component.Value.Settings,
                        Features = component.Value.GetFeatures().ToDictionary(i => i.GetType().Name, i => i)
                    };

                    response.Add(component.Key, componentConfiguration);
                }
            }

            apiContext.Result = JObject.FromObject(response);
        }
    }
}
