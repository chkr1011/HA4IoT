using System;
using System.Collections.Generic;
using Windows.Data.Json;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services;

namespace HA4IoT.SynonymsService
{
    public class SynonymsService : IService
    {
        private readonly Dictionary<IdBase, HashSet<string>> _synonyms = new Dictionary<IdBase, HashSet<string>>();

        public void AddSynonym(IdBase id, string synonym)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (synonym == null) throw new ArgumentNullException(nameof(synonym));

            HashSet<string> synonyms;
            if (!_synonyms.TryGetValue(id, out synonyms))
            {
                synonyms = new HashSet<string>();
                _synonyms.Add(id, synonyms);
            }

            synonyms.Add(synonym);
        }

        public JsonObject ExportStatusToJsonObject()
        {
            ////var result = new JsonObject();
            ////foreach (var item in _synonyms)
            ////{
            ////    var values = new JsonArray();
            ////    foreach (var synonym in item.Value)
            ////    {
            ////        values.Add(JsonValue.CreateStringValue(synonym));
            ////    }

            ////    result.SetNamedArray(item.Key.Value, values);
            ////}

            ////return result;
           
            return new JsonObject();
        }
    }
}
