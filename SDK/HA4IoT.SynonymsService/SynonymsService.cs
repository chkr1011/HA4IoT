using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Services;

namespace HA4IoT.SynonymsService
{
    public class SynonymsService : IService
    {
        private readonly Dictionary<ActuatorId, HashSet<string>> _actuatorSynonyms = new Dictionary<ActuatorId, HashSet<string>>();

        public void AddSynonym(ActuatorId actuatorId, string synonym)
        {
            if (actuatorId == null) throw new ArgumentNullException(nameof(actuatorId));
            if (synonym == null) throw new ArgumentNullException(nameof(synonym));

            HashSet<string> synonyms;
            if (!_actuatorSynonyms.TryGetValue(actuatorId, out synonyms))
            {
                synonyms = new HashSet<string>();
                _actuatorSynonyms.Add(actuatorId, synonyms);
            }

            synonyms.Add(synonym);
        }
    }
}
