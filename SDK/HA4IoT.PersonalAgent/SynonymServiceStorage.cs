using System;
using System.Collections.Generic;
using System.IO;
using Windows.Data.Json;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Networking;

namespace HA4IoT.PersonalAgent
{
    public class SynonymServiceStorage
    {
        private readonly string _areaSynonymsFilename;
        private readonly string _componentSynonymsFilename;
        private readonly string _componentStateSynonymsFilename;

        public SynonymServiceStorage()
        {
            var rootPath = StoragePath.WithFilename("Services", "SynonymService");

            _areaSynonymsFilename = Path.Combine(rootPath, "Areas.json");
            _componentSynonymsFilename = Path.Combine(rootPath, "Components.json");
            _componentStateSynonymsFilename = Path.Combine(rootPath, "ComponentStates.json");
        }

        public void PersistAreaSynonyms(Dictionary<AreaId, HashSet<string>> synonyms)
        {
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));

            File.WriteAllText(_areaSynonymsFilename, ConvertAreaSynonymsToJsonObject(synonyms).Stringify());
        }

        public JsonObject ConvertAreaSynonymsToJsonObject(Dictionary<AreaId, HashSet<string>> synonyms)
        {
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));

            var result = new JsonObject();
            foreach (var synonym in synonyms)
            {
                result.SetNamedArray(synonym.Key.Value, ConvertSynonymsToJsonArray(synonym.Value));
            }

            return result;
        }

        public void PersistComponentSynonyms(Dictionary<ComponentId, HashSet<string>> synonyms)
        {
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));
            
            File.WriteAllText(_componentSynonymsFilename, ConvertComponentSynonymsToJsonObject(synonyms).Stringify());
        }

        public JsonObject ConvertComponentSynonymsToJsonObject(Dictionary<ComponentId, HashSet<string>> synonyms)
        {
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));
            
            var result = new JsonObject();
            foreach (var synonym in synonyms)
            {
                result.SetNamedArray(synonym.Key.Value, ConvertSynonymsToJsonArray(synonym.Value));
            }

            return result;
        }

        public void PersistComponentStateSynonyms(Dictionary<IComponentState, HashSet<string>> synonyms)
        {
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));
            
            File.WriteAllText(_componentSynonymsFilename, ConvertComponentStateSynonymsToJsonArray(synonyms).Stringify());
        }

        public JsonArray ConvertComponentStateSynonymsToJsonArray(Dictionary<IComponentState, HashSet<string>> synonyms)
        {
            if (synonyms == null) throw new ArgumentNullException(nameof(synonyms));

            var result = new JsonArray();
            foreach (var synonym in synonyms)
            {
                var item = new JsonObject();
                item.SetNamedObject("ComponentState", synonym.Key.ToJsonObject());
                item.SetNamedValue("Synonyms", ConvertSynonymsToJsonArray(synonym.Value));

                result.Add(item);
            }

            return result;
        }

        public void LoadAreaSynonymsTo(Dictionary<AreaId, HashSet<string>> target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            string fileContent = File.ReadAllText(_areaSynonymsFilename);
            JsonObject _source = JsonObject.Parse(fileContent);

            foreach (var key in _source.Keys)
            {
                var areaId = new AreaId(key);
                HashSet<string> synonyms = ConvertJsonArrayToSynonyms(_source.GetNamedArray(key));

                target[areaId] = synonyms;
            }
        }

        public void LoadComponentSynonymsTo(Dictionary<ComponentId, HashSet<string>> target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            string fileContent = File.ReadAllText(_areaSynonymsFilename);
            JsonObject _source = JsonObject.Parse(fileContent);

            foreach (var key in _source.Keys)
            {
                var componentId = new ComponentId(key);
                HashSet<string> synonyms = ConvertJsonArrayToSynonyms(_source.GetNamedArray(key));

                target[componentId] = synonyms;
            }
        }

        private HashSet<string> ConvertJsonArrayToSynonyms(JsonArray source)
        {
            var result = new HashSet<string>();
            foreach (var synonym in source)
            {
                result.Add(synonym.GetString());
            }

            return result;
        }

        private JsonArray ConvertSynonymsToJsonArray(HashSet<string> synonyms)
        {
            var result = new JsonArray();
            foreach (var synonym in synonyms)
            {
                result.Add(JsonValue.CreateStringValue(synonym));
            }

            return result;
        }
    }
}