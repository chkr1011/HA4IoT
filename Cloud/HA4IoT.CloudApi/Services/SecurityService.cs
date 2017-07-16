using System;
using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json;

namespace HA4IoT.CloudApi.Services
{
    public class SecurityService
    {
        private readonly Dictionary<Guid, string> _allowedControllers = new Dictionary<Guid, string>();
        private readonly Dictionary<string, Guid> _amazonUserIdMappings = new Dictionary<string, Guid>();

        public SecurityService()
        {
            try
            {
                var allowedControllers = ConfigurationManager.AppSettings["AllowedControllers"] ?? string.Empty;
                if (!string.IsNullOrEmpty(allowedControllers))
                {
                    _allowedControllers = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(allowedControllers);
                }

                var amazonUserIdMappings = ConfigurationManager.AppSettings["AmazonUserIdMappings"] ?? string.Empty;
                if (!string.IsNullOrEmpty(amazonUserIdMappings))
                {
                    _amazonUserIdMappings = JsonConvert.DeserializeObject<Dictionary<string, Guid>>(amazonUserIdMappings);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        public bool CredentialsAreValid(Guid controllerId, string apiKey)
        {
            if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));

            string expectedApiKey;
            if (!_allowedControllers.TryGetValue(controllerId, out expectedApiKey))
            {
                return false;
            }

            return string.CompareOrdinal(apiKey, expectedApiKey) == 0;
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