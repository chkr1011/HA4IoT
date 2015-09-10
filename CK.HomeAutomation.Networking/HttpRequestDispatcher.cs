using System;
using System.Collections.Generic;
using System.IO;

namespace CK.HomeAutomation.Networking
{
    public class HttpRequestDispatcher
    {
        private readonly Dictionary<string, HttpRequestController> _controllers =
            new Dictionary<string, HttpRequestController>(StringComparer.OrdinalIgnoreCase);

        private readonly HttpServer _server;

        public HttpRequestDispatcher(HttpServer server)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            _server = server;
        }

        public HttpRequestController GetController(string name)
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

        public void MapDirectory(string controllerName, string rootDirectory)
        {
            var controller = GetController(controllerName);
            controller.Handle(HttpMethod.Get, "").WithAnySubUrl().Using(c =>
            {
                string relativeUrl = c.Request.Uri.Trim('/');
                int index = relativeUrl.IndexOf(controllerName, StringComparison.OrdinalIgnoreCase);
                if (index == -1)
                {
                    return;
                }

                relativeUrl = relativeUrl.Substring(index + controllerName.Length).Replace("/", "\\").Replace("%20", " ");
                string filename = rootDirectory + relativeUrl;

                var mimeTypeProvider = new MimeTypeProvider();
                c.Response.MimeType = mimeTypeProvider.GetMimeTypeOfFile(filename);

                c.Response.Body.Append(File.ReadAllText(filename));
            });
        }
    }
}