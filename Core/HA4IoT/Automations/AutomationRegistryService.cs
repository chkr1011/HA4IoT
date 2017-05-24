using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Settings;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Automations
{
    public class AutomationRegistryService : ServiceBase, IAutomationRegistryService
    {
        private readonly ISettingsService _settingsService;
        private readonly Dictionary<string, IAutomation> _automations = new Dictionary<string, IAutomation>();

        public AutomationRegistryService(
            ISettingsService settingsService,
            ISystemEventsService systemEventsService,
            ISystemInformationService systemInformationService, 
            IApiDispatcherService apiService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            if (systemEventsService == null) throw new ArgumentNullException(nameof(systemEventsService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));

            systemEventsService.StartupCompleted += (s, e) =>
            {
                systemInformationService.Set("Automations/Count", _automations.Count);
            };

            apiService.StatusRequested += HandleApiStatusRequest;
        }

        public void AddAutomation(IAutomation automation)
        {
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            _automations.Add(automation.Id, automation);
        }

        public IList<TAutomation> GetAutomations<TAutomation>() where TAutomation : IAutomation
        {
            return _automations.Values.OfType<TAutomation>().ToList();
        }

        public TAutomation GetAutomation<TAutomation>(string id) where TAutomation : IAutomation
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            return (TAutomation)_automations[id];
        }

        public IList<IAutomation> GetAutomations()
        {
            return _automations.Values.ToList();
        }

        private void HandleApiStatusRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            var automations = new JObject();
            foreach (var automation in _automations.Values.ToList())
            {
                var status = new JObject
                {
                    ["Settings"] = _settingsService.GetRawSettings(automation),
                    //["State"] = JToken.FromObject(automation.GetState().Serialize())
                };

                automations[automation.Id] = status;
            }

            e.ApiContext.Result["Automations"] = automations;
        }
    }
}
