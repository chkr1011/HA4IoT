using System;
using System.Collections.Generic;

namespace CK.HomeAutomation.Networking
{
    public class HttpRequestController : IHttpRequestController
    {
        private readonly string _baseUri;
        private readonly List<HttpRequestDispatcherAction> _handlers = new List<HttpRequestDispatcherAction>();

        public HttpRequestController(string name, HttpServer server)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (server == null) throw new ArgumentNullException(nameof(server));

            _baseUri = name;
            server.RequestReceived += ExecuteActions;
        }

        public HttpRequestDispatcherAction Handle(HttpMethod method, string uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            var action = new HttpRequestDispatcherAction(method, uri);
            _handlers.Add(action);

            return action;
        }

        private void ExecuteActions(object sender, RequestReceivedEventArgs e)
        {
            string requestUri = e.Context.Request.Uri.Trim('/');

            if (!requestUri.StartsWith(_baseUri, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string relativeUri = requestUri.Substring(_baseUri.Length).Trim('/');
            foreach (var handler in _handlers)
            {
                if (!handler.Method.Equals(e.Context.Request.Method))
                {
                    continue;
                }

                if (e.Context.Request.PlainBody.Length == 0 && handler.IsJsonBodyRequired)
                {
                    continue;
                }

                if (handler.HandleRequestsWithDifferentSubUrl)
                {
                    if (!relativeUri.StartsWith(handler.Uri, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }
                else
                {
                    if (!handler.Uri.Equals(relativeUri, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }
                
                handler.Action(e.Context);
                e.IsHandled = true;

                return;
            }
        }
    }
}
