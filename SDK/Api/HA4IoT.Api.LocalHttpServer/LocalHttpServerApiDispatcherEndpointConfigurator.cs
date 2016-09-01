using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Networking;
using HA4IoT.Networking.Http;

namespace HA4IoT.Api.LocalHttpServer
{
    public class LocalHttpServerApiDispatcherEndpointConfigurator : IConfigurator
    {
        private readonly IApiService _apiService;
        private readonly HttpServer _httpServer;

        public LocalHttpServerApiDispatcherEndpointConfigurator(IApiService apiService, HttpServer httpServer)
        {
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));
            if (httpServer == null) throw new ArgumentNullException(nameof(httpServer));

            _apiService = apiService;
            _httpServer = httpServer;
        }

        public void Execute()
        {
            var httpApiDispatcherEndpoint = new LocalHttpServerApiDispatcherEndpoint(_httpServer);
            _apiService.RegisterEndpoint(httpApiDispatcherEndpoint);
        }
    }
}
