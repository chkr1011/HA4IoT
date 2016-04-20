using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services;
using HA4IoT.Core;

namespace HA4IoT.PersonalAgent
{
    public class SynonymService : IService
    {
        private readonly IApiController _apiController;
        private readonly Dictionary<IComponentState, HashSet<string>> _componentStateSynonyms = new Dictionary<IComponentState, HashSet<string>>(); 
        private readonly Dictionary<ComponentId, HashSet<string>> _componentSynonyms = new Dictionary<ComponentId, HashSet<string>>();

        public SynonymService(IApiController apiController)
        {
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            _apiController = apiController;

            RegisterDefaultComponentStateSynonyms();
        }

        public void AddSynonymForComponent(Enum areaId, Enum componentId, params string[] synonyms)
        {
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));
            
            AddSynonymForComponent(AreaIdFactory.Create(areaId), componentId, synonyms);
        }

        public void AddSynonymForComponent(AreaId areaId, Enum componentId, params string[] synonyms)
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));

            ComponentId id = ComponentIdFactory.Create(areaId, componentId);
            AddSynonymForComponent(id, synonyms);
        }

        public void AddSynonymForComponent(ComponentId componentId, params string[] synonyms)
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));

            HashSet<string> existingSynonyms;
            if (!_componentSynonyms.TryGetValue(componentId, out existingSynonyms))
            {
                existingSynonyms = new HashSet<string>();
                _componentSynonyms.Add(componentId, existingSynonyms);
            }

            foreach (string synonym in synonyms)
            {
                existingSynonyms.Add(synonym);
            }
        }

        public void AddSynonymForComponentState(IComponentState componentState, params string[] synonyms)
        {
            if (componentState == null) throw new ArgumentNullException(nameof(componentState));
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));

            HashSet<string> existingSynonyms;
            if (!_componentStateSynonyms.TryGetValue(componentState, out existingSynonyms))
            {
                existingSynonyms = new HashSet<string>();
                _componentStateSynonyms.Add(componentState, existingSynonyms);
            }

            foreach (string synonym in synonyms)
            {
                existingSynonyms.Add(synonym);
            }
        }

        public IList<ComponentId> GetComponentsBySynonym(string synonym)
        {
            if (synonym == null) throw new ArgumentNullException(nameof(synonym));

            return _componentSynonyms.Where(i => i.Value.Any(s => s.Equals(synonym, StringComparison.CurrentCultureIgnoreCase))).Select(i => i.Key).ToList();
        }

        public IList<IComponentState> GetComponentStatesBySynonym(string synonym)
        {
            if (synonym == null) throw new ArgumentNullException(nameof(synonym));

            return _componentStateSynonyms.Where(i => i.Value.Any(s => s.Equals(synonym, StringComparison.CurrentCultureIgnoreCase))).Select(i => i.Key).ToList();
        }

        private void RegisterDefaultComponentStateSynonyms()
        {
            AddSynonymForComponentState(BinaryStateId.Off, "aus", "ab", "stop", "stoppe", "halt", "off");
            AddSynonymForComponentState(BinaryStateId.On, "an", "ein", "go", "on");

            AddSynonymForComponentState(RollerShutterStateId.MovingUp, "rauf", "hoch", "up");

            AddSynonymForComponentState(RollerShutterStateId.MovingDown, "runter", "herunter", "down");            
        }

        public JsonObject ExportStatusToJsonObject()
        {
            return new JsonObject();
        }
    }
}
