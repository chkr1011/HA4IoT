using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Networking;
using HA4IoT.Contracts.Networking.Http;

namespace HA4IoT.Networking.Http
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

        public IHttpRequestDispatcherAction HandleGet(string uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            var action = new HttpRequestDispatcherAction(HttpMethod.Get, uri);
            _handlers.Add(action);

            return action;
        }

        public IHttpRequestDispatcherAction HandlePost(string uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            var action = new HttpRequestDispatcherAction(HttpMethod.Post, uri);
            _handlers.Add(action);

            return action;
        }

        public IHttpRequestDispatcherAction HandlePatch(string uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            var action = new HttpRequestDispatcherAction(HttpMethod.Patch, uri);
            _handlers.Add(action);

            return action;
        }

        private void ExecuteActions(object sender, HttpRequestReceivedEventArgs e)
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
                
                if (handler.HandleRequestsWithDifferentSubUrl)
                {
                    if (!relativeUri.StartsWith(handler.Uri))
                    {
                        continue;
                    }
                }
                else
                {
                    if (!handler.Uri.Equals(relativeUri))
                    {
                        continue;
                    }
                }

                InvokeHandlerAction(handler, e.Context);
                e.IsHandled = true;
                return;
            }
        }

        private void InvokeHandlerAction(HttpRequestDispatcherAction handler, HttpContext context)
        {
            try
            {
                handler.Handler(context);
            }
            catch (BadRequestException)
            {
                context.Response.StatusCode = HttpStatusCode.BadRequest;
            }
        }
    }
}
