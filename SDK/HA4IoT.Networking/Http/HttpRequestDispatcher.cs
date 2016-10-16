using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Networking.Http;

namespace HA4IoT.Networking.Http
{
    public class HttpRequestDispatcher : IHttpRequestDispatcher
    {
        private readonly Dictionary<string, HttpRequestController> _controllers =
            new Dictionary<string, HttpRequestController>(StringComparer.OrdinalIgnoreCase);

        private readonly HttpServer _server;

        public HttpRequestDispatcher(HttpServer server)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            _server = server;
        }

        public IHttpRequestController GetController(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            HttpRequestController controller;
            if (!_controllers.TryGetValue(name, out controller))
            {
                controller = new HttpRequestController(name, _server);
                _controllers.Add(name, controller);
            }

            return controller;
        }

        public void MapFolder(string name, string rootFolder)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (name == null) throw new ArgumentNullException(nameof(rootFolder));

            if (_controllers.ContainsKey(name))
            {
                throw new InvalidOperationException("The controller is already registered");
            }

            var controller = new MappedFolderController(name, rootFolder, _server);
            _controllers.Add(name, controller);
        }
    }
}