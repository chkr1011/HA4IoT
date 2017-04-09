using System;
using System.Collections.Generic;
using System.Net;

namespace HA4IoT.Networking.Http.Controllers
{
    public class HttpRequestController
    {
        private readonly List<HttpRequestControllerAction> _handlers = new List<HttpRequestControllerAction>();
        private readonly string _baseUri;

        public HttpRequestController(string name, HttpServer server)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (server == null) throw new ArgumentNullException(nameof(server));

            _baseUri = name;
            server.HttpRequestReceived += ExecuteActions;
        }

        public HttpRequestControllerAction Handle(HttpMethod method, string uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            var action = new HttpRequestControllerAction(method, uri);
            _handlers.Add(action);

            return action;
        }

        private void ExecuteActions(object sender, HttpRequestReceivedEventArgs e)
        {
            var requestUri = e.Context.Request.Uri.Trim('/');

            if (!requestUri.StartsWith(_baseUri, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var relativeUri = requestUri.Substring(_baseUri.Length).Trim('/');

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

        private static void InvokeHandlerAction(HttpRequestControllerAction handler, HttpContext context)
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
