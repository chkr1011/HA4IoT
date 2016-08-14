using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Services.Areas
{
    public class AreaService : ServiceBase, IAreaService
    {
        private readonly AreaCollection _areas = new AreaCollection();

        private readonly IComponentService _componentService;
        private readonly IAutomationService _automationService;

        public AreaService(IComponentService componentService, IAutomationService automationService)
        {
            if (componentService == null) throw new ArgumentNullException(nameof(componentService));
            if (automationService == null) throw new ArgumentNullException(nameof(automationService));

            _componentService = componentService;
            _automationService = automationService;
        }

        public IArea CreateArea(AreaId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var area = new Area(id, _componentService, _automationService);
            _areas.AddUnique(id, area);

            return area;
        }

        public void AddArea(IArea area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            _areas.AddUnique(area.Id, area);
        }

        public IArea GetArea(AreaId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            return _areas.Get(id);
        }

        public IList<IArea> GetAreas()
        {
            return _areas.GetAll();
        }
    }
}
