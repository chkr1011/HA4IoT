using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Services.Automations
{
    public class AutomationService : ServiceBase, IAutomationService
    {
        private readonly AutomationCollection _automations = new AutomationCollection();

        public AutomationService(
            ISystemEventsService systemEventsService,
            ISystemInformationService systemInformationService)
        {
            if (systemEventsService == null) throw new ArgumentNullException(nameof(systemEventsService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));

            systemEventsService.StartupCompleted += (s, e) =>
            {
                systemInformationService.Set("Automations/Count", _automations.GetAll().Count);
            };
        }

        public void AddAutomation(IAutomation automation)
        {
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            _automations.AddOrUpdate(automation.Id, automation);
        }

        public IList<TAutomation> GetAutomations<TAutomation>() where TAutomation : IAutomation
        {
            return _automations.GetAll<TAutomation>();
        }

        public TAutomation GetAutomation<TAutomation>(AutomationId id) where TAutomation : IAutomation
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            return _automations.Get<TAutomation>(id);
        }

        public IList<IAutomation> GetAutomations()
        {
            return _automations.GetAll();
        }
    }
}
