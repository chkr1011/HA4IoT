using System;
using System.Collections.Generic;
using System.IO;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using Newtonsoft.Json.Linq;

namespace HA4IoT.PersonalAgent
{
    public class SynonymServiceStorage
    {
        private readonly string _areaSynonymsFilename;
        private readonly string _componentSynonymsFilename;
        private readonly string _componentStateSynonymsFilename;

        public SynonymServiceStorage()
        {
            var rootPath = Path.Combine(StoragePath.Root, "Services", "SynonymService");
            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }

            _areaSynonymsFilename = Path.Combine(rootPath, "Areas.json");
            _componentSynonymsFilename = Path.Combine(rootPath, "Components.json");
            _componentStateSynonymsFilename = Path.Combine(rootPath, "ComponentStates.json");
        }

        public void PersistAreaSynonyms(Dictionary<AreaId, HashSet<string>> synonyms)
        {
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));

            File.WriteAllText(_areaSynonymsFilename, ConvertAreaSynonymsToJsonObject(synonyms).ToString());
        }

        public JObject ConvertAreaSynonymsToJsonObject(Dictionary<AreaId, HashSet<string>> synonyms)
        {
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));

            var result = new JObject();
            foreach (var synonym in synonyms)
            {
                result[synonym.Key.Value] = ConvertSynonymsToJsonArray(synonym.Value);
            }

            return result;
        }

        public void PersistComponentSynonyms(Dictionary<ComponentId, HashSet<string>> synonyms)
        {
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));
            
            File.WriteAllText(_componentSynonymsFilename, ConvertComponentSynonymsToJsonObject(synonyms).ToString());
        }

        public JObject ConvertComponentSynonymsToJsonObject(Dictionary<ComponentId, HashSet<string>> synonyms)
        {
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));
            
            var result = new JObject();
            foreach (var synonym in synonyms)
            {
                result[synonym.Key.Value] = ConvertSynonymsToJsonArray(synonym.Value);
            }

            return result;
        }

        public void PersistComponentStateSynonyms(Dictionary<ComponentState, HashSet<string>> synonyms)
        {
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));
            
            File.WriteAllText(_componentStateSynonymsFilename, ConvertComponentStateSynonymsToJsonArray(synonyms).ToString());
        }

        public JArray ConvertComponentStateSynonymsToJsonArray(Dictionary<ComponentState, HashSet<string>> synonyms)
        {
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));

            var result = new JArray();
            foreach (var synonym in synonyms)
            {
                var item = new JObject
                {
                    ["ComponentState"] = synonym.Key.JToken,
                    ["Synonyms"] = ConvertSynonymsToJsonArray(synonym.Value)
                };

                result.Add(item);
            }

            return result;
        }

        public void LoadAreaSynonymsTo(Dictionary<AreaId, HashSet<string>> target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (!File.Exists(_areaSynonymsFilename))
            {
                return;
            }

            var fileContent = File.ReadAllText(_areaSynonymsFilename);
            var source = JObject.Parse(fileContent);

            foreach (var property in source.Properties())
            {
                var areaId = new AreaId(property.Name);
                var synonyms = ConvertJsonArrayToSynonyms(property.Value.ToObject<JArray>());

                target[areaId] = synonyms;
            }
        }

        public void LoadComponentSynonymsTo(Dictionary<ComponentId, HashSet<string>> target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (!File.Exists(_componentSynonymsFilename))
            {
                return;
            }

            var fileContent = File.ReadAllText(_componentSynonymsFilename);
            var source = JObject.Parse(fileContent);

            foreach (var property in source.Properties())
            {
                var componentId = new ComponentId(property.Name);
                var synonyms = ConvertJsonArrayToSynonyms(property.Value.ToObject<JArray>());

                target[componentId] = synonyms;
            }
        }

        private HashSet<string> ConvertJsonArrayToSynonyms(JArray source)
        {
            var result = new HashSet<string>();
            foreach (var synonym in source)
            {
                result.Add(synonym.ToObject<string>());
            }

            return result;
        }

        private JArray ConvertSynonymsToJsonArray(HashSet<string> synonyms)
        {
            var result = new JArray();
            foreach (var synonym in synonyms)
            {
                result.Add(JToken.FromObject(synonym));
            }

            return result;
        }
    }
}