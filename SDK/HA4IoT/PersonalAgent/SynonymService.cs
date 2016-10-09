using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services;

namespace HA4IoT.PersonalAgent
{
    public class SynonymService : ServiceBase
    {
        private readonly IComponentService _componentService;
        private readonly Dictionary<AreaId, HashSet<string>> _areaSynonyms = new Dictionary<AreaId, HashSet<string>>();
        private readonly Dictionary<ComponentId, HashSet<string>> _componentSynonyms = new Dictionary<ComponentId, HashSet<string>>();
        private readonly Dictionary<ComponentState, HashSet<string>> _componentStateSynonyms = new Dictionary<ComponentState, HashSet<string>>();
        private readonly SynonymServiceStorage _storage;

        public SynonymService(IComponentService componentService)
        {
            if (componentService == null) throw new ArgumentNullException(nameof(componentService));

            _componentService = componentService;
            _storage = new SynonymServiceStorage();
        }

        public void LoadPersistedSynonyms()
        {
            _storage.LoadAreaSynonymsTo(_areaSynonyms);
            _storage.LoadComponentSynonymsTo(_componentSynonyms);
            //_storage.Load
        }

        public bool TryLoadPersistedSynonyms()
        {
            try
            {
                LoadPersistedSynonyms();
                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error while loading persisted synonyms.");
                return false;
            }
        }

        public void AddSynonymsForComponent(Enum areaId, Enum componentId, params string[] synonyms)
        {
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));
            
            AddSynonymsForComponent(AreaIdGenerator.Generate(areaId), componentId, synonyms);
        }

        public void AddSynonymsForComponent(AreaId areaId, Enum componentId, params string[] synonyms)
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));

            AddSynonymsForComponent(ComponentIdGenerator.Generate(areaId, componentId), synonyms);
        }

        public void AddSynonymsForComponent(ComponentId componentId, params string[] synonyms)
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));

            AddSynonyms(_componentSynonyms, componentId, synonyms);
            _storage.PersistComponentSynonyms(_componentSynonyms);
        }

        public void AddSynonymsForComponentState(ComponentState componentState, params string[] synonyms)
        {
            if (componentState == null) throw new ArgumentNullException(nameof(componentState));
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));

            AddSynonyms(_componentStateSynonyms, componentState, synonyms);
            _storage.PersistComponentStateSynonyms(_componentStateSynonyms);
        }

        public void AddSynonymsForArea(Enum areaId, params string[] synonyms)
        {
            if (areaId == null) throw new ArgumentNullException(nameof(areaId));
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));

            AddSynonyms(_areaSynonyms, AreaIdGenerator.Generate(areaId), synonyms);
            _storage.PersistAreaSynonyms(_areaSynonyms);
        }

        public void AddSynonymsForArea(AreaId areaId, params string[] synonyms)
        {
            if (areaId == null) throw new ArgumentNullException(nameof(areaId));
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));

            AddSynonyms(_areaSynonyms, areaId, synonyms);
            _storage.PersistAreaSynonyms(_areaSynonyms);
        }

        public IList<AreaId> GetAreaIdsBySynonym(string synonym)
        {
            if (synonym == null) throw new ArgumentNullException(nameof(synonym));

            return _areaSynonyms.Where(i => i.Value.Any(s => s.Equals(synonym, StringComparison.CurrentCultureIgnoreCase))).Select(i => i.Key).ToList();
        }

        public IList<ComponentId> GetComponentIdsBySynonym(string synonym)
        {
            if (synonym == null) throw new ArgumentNullException(nameof(synonym));

            return _componentSynonyms.Where(i => i.Value.Any(s => s.Equals(synonym, StringComparison.CurrentCultureIgnoreCase))).Select(i => i.Key).ToList();
        }

        public IList<ComponentState> GetComponentStatesBySynonym(string synonym)
        {
            if (synonym == null) throw new ArgumentNullException(nameof(synonym));

            return _componentStateSynonyms.Where(i => i.Value.Any(s => s.Equals(synonym, StringComparison.CurrentCultureIgnoreCase))).Select(i => i.Key).ToList();
        }

        public void HandleApiCall(IApiContext apiContext)
        {
            ////apiContext.Response.SetValue("AreaSynonyms", _storage.ConvertAreaSynonymsToJsonObject(_areaSynonyms));

            ////apiContext.Response.SetValue("ComponentSynonyms",
            ////    _storage.ConvertComponentSynonymsToJsonObject(_componentSynonyms));

            ////apiContext.Response.SetValue("ComponentStateSynonyms",
            ////    _storage.ConvertComponentStateSynonymsToJsonArray(_componentStateSynonyms));
        }

        public void RegisterDefaultComponentStateSynonyms()
        {
            AddSynonymsForComponentState(BinaryStateId.Off, "aus", "ausschalten", "ab", "abschalten", "stop", "stoppe", "halt", "anhalten", "off");
            AddSynonymsForComponentState(BinaryStateId.On, "an", "anschalten", "ein", "einschalten", "on");

            AddSynonymsForComponentState(RollerShutterStateId.MovingUp, "rauf", "herauf", "hoch", "oben", "öffne", "öffnen", "up");
            AddSynonymsForComponentState(RollerShutterStateId.MovingDown, "runter", "herunter", "unten", "schließe", "schließen", "down");

            foreach (var temperatureSensor in _componentService.GetComponents<ITemperatureSensor>())
            {
                AddSynonymsForComponent(temperatureSensor.Id, "Temperatur", "warm", "kalt", "temperature");
            }

            foreach (var humiditySensor in _componentService.GetComponents<IHumiditySensor>())
            {
                AddSynonymsForComponent(humiditySensor.Id, "feucht", "Feuchtigkeit", "trocken", "humidity");
            }

            foreach (var rollerShutter in _componentService.GetComponents<IRollerShutter>())
            {
                AddSynonymsForComponent(rollerShutter.Id, "Rollo", "Rollade", "trocken");
            }
        }

        private void AddSynonyms<TValue>(IDictionary<TValue, HashSet<string>> target, TValue value, params string[] synonyms)
        {
            HashSet<string> existingSynonyms;
            if (!target.TryGetValue(value, out existingSynonyms))
            {
                existingSynonyms = new HashSet<string>();
                target.Add(value, existingSynonyms);
            }

            foreach (string synonym in synonyms)
            {
                existingSynonyms.Add(synonym);
            }
        }
    }
}
