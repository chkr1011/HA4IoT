using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Api.AzureCloud
{
    public class AzureCloudApiDispatcherEndpointConfigurator : IConfigurator
    {
        private readonly IApiService _apiService;

        public AzureCloudApiDispatcherEndpointConfigurator(IApiService apiService)
        {
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));

            _apiService = apiService;
        }

        public void Execute()
        {
            var azureCloudApiDispatcherEndpoint = new AzureCloudApiDispatcherEndpoint();

            if (azureCloudApiDispatcherEndpoint.TryInitializeFromConfigurationFile(
                StoragePath.WithFilename("AzureCloudApiDispatcherEndpointSettings.json")))
            {
                _apiService.RegisterEndpoint(azureCloudApiDispatcherEndpoint);

                Log.Info("Azure API endpoint created.");
            }
            else
            {
                Log.Verbose("Azure API endpoint not available.");
            }
            
        }
    }
}
