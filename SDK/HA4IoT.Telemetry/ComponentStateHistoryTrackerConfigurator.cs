using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Telemetry
{
    public class ComponentStateHistoryTrackerConfigurator : IConfigurator
    {
        private readonly IComponentService _componentService;
        private readonly IApiService _apiService;

        public ComponentStateHistoryTrackerConfigurator(IComponentService componentService, IApiService apiService)
        {
            if (componentService == null) throw new ArgumentNullException(nameof(componentService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));

            _componentService = componentService;
            _apiService = apiService;
        }

        public void Execute()
        {
            foreach (var component in _componentService.GetComponents())
            {
                var history = new ComponentStateHistoryTracker(component);
                history.ExposeToApi(_apiService);
            }
        }
    }
}
