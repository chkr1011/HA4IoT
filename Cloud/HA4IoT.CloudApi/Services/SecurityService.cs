using System;
using System.Collections.Generic;
using System.Configuration;

namespace HA4IoT.CloudApi.Services
{
    public class SecurityService
    {
        public SecurityService()
        {
            ApiKey = ConfigurationManager.AppSettings["ApiKey"];

            var allowedControllerIds = ConfigurationManager.AppSettings["AllowedControllerIds"] ?? string.Empty;
            foreach (var allowedControllerId in allowedControllerIds.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                AllowedControllerIds.Add(Guid.Parse(allowedControllerId));
            }
        }

        public string ApiKey { get; }

        public HashSet<Guid> AllowedControllerIds { get; } = new HashSet<Guid>();
    }
}