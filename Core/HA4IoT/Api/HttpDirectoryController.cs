using System;
using System.IO;
using Windows.Web.Http;
using HA4IoT.Networking.Http;

namespace HA4IoT.Api
{
    public class HttpDirectoryController
    {
        private readonly HttpServer _httpServer;
        private readonly string _baseAddress;
        private readonly string _rootDirectory;

        public HttpDirectoryController(string name, string rootDirectory, HttpServer httpServer)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            _baseAddress = "/" + name + "/";
            _rootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
            _httpServer = httpServer ?? throw new ArgumentNullException(nameof(httpServer));
        }

        public string DefaultFile { get; } = "Index.html";

        public void Enable()
        {
            if (!Directory.Exists(_rootDirectory))
            {
                Directory.CreateDirectory(_rootDirectory);
            }

            _httpServer.HttpRequestReceived += OnHttpRequestReceived;
        }

        private void OnHttpRequestReceived(object sender, HttpRequestReceivedEventArgs e)
        {
            if (!e.Context.Request.Uri.StartsWith(_baseAddress, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            e.IsHandled = true;

            string filename;
            if (!TryGetFilename(e.Context, out filename))
            {
                e.Context.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            if (File.Exists(filename))
            {
                e.Context.Response.Body = File.ReadAllBytes(filename);
                e.Context.Response.MimeType = MimeTypeProvider.GetMimeTypeFromFile(filename);
            }
            else
            {
                e.Context.Response.StatusCode = HttpStatusCode.NotFound;
            }
        }

        private bool TryGetFilename(HttpContext httpContext, out string filename)
        {
            var relativeUrl = httpContext.Request.Uri.Substring(_baseAddress.Length);
            relativeUrl = relativeUrl.TrimStart('/');

            if (relativeUrl.EndsWith("/"))
            {
                relativeUrl += DefaultFile;
            }

            relativeUrl = relativeUrl.Trim('/');
            relativeUrl = relativeUrl.Replace("/", @"\");

            filename = Path.Combine(_rootDirectory, relativeUrl);
            return true;
        }
    }
}
