using System;
using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json;

namespace HA4IoT.CloudApi.Services
{
    public class SecurityService
    {
        private readonly Dictionary<string, Guid> _amazonUserIdMappings = new Dictionary<string, Guid>();
        private readonly HashSet<Guid> _allowedControllerIds = new HashSet<Guid>();
        private readonly string _apiKey;

        public SecurityService()
        {
            _apiKey = ConfigurationManager.AppSettings["ApiKey"];

            var allowedControllerIds = ConfigurationManager.AppSettings["AllowedControllerIds"] ?? string.Empty;
            if (!string.IsNullOrEmpty(allowedControllerIds))
            {
                _allowedControllerIds = JsonConvert.DeserializeObject<HashSet<Guid>>(allowedControllerIds);
            }

            var amazonUserIdMappings = ConfigurationManager.AppSettings["AmazonUserIdMappings"] ?? string.Empty;
            if (!string.IsNullOrEmpty(amazonUserIdMappings))
            {
                _amazonUserIdMappings = JsonConvert.DeserializeObject<Dictionary<string, Guid>>(amazonUserIdMappings);
            }
        }
        
        public bool ApiKeyIsValid(string apiKey)
        {
            if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));

            return string.CompareOrdinal(_apiKey, apiKey) == 0;
        }

        public bool ControllerIsAllowed(Guid controllerId)
        {
            return _allowedControllerIds.Contains(controllerId);
        }

        public Guid? GetControllerUidFromAmazonUserId(string azureUserId)
        {
            Guid controllerId;
            if (!_amazonUserIdMappings.TryGetValue(azureUserId, out controllerId))
            {
                return null;
            }

            return controllerId;
        }
    }
}