using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Networking.Http;

namespace HA4IoT.Api.LocalHttpServer
{
    public class AppFolderConfigurator : IConfigurator
    {
        private readonly HttpServer _httpServer;

        public AppFolderConfigurator(HttpServer httpServer)
        {
            if (httpServer == null) throw new ArgumentNullException(nameof(httpServer));

            _httpServer = httpServer;
        }

        public void Execute()
        {
            var httpRequestDispatcher = new HttpRequestDispatcher(_httpServer);
            httpRequestDispatcher.MapFolder("App", StoragePath.AppRoot);
            httpRequestDispatcher.MapFolder("ManagementApp", StoragePath.ManagementAppRoot);
            httpRequestDispatcher.MapFolder("Storage", StoragePath.Root);
        }
    }
}
