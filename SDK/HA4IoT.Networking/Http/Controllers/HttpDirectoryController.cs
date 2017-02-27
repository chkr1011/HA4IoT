using System;
using System.IO;
using System.Net;

namespace HA4IoT.Networking.Http.Controllers
{
    public class HttpDirectoryController : HttpRequestController
    {
        private readonly string _name;
        private readonly string _rootDirectory;

        public HttpDirectoryController(string name, string rootDirectory, HttpServer httpServer)
            : base(name, httpServer)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (rootDirectory == null) throw new ArgumentNullException(nameof(rootDirectory));

            _name = name;
            _rootDirectory = rootDirectory;
        }

        public string DefaultFile { get; } = "Index.html";

        public void Enable()
        {
            if (!Directory.Exists(_rootDirectory))
            {
                Directory.CreateDirectory(_rootDirectory);
            }

            Handle(HttpMethod.Get, string.Empty).WithAnySubUrl().Using(HandleGet);
            Handle(HttpMethod.Post, string.Empty).WithAnySubUrl().Using(HandlePost);
        }

        private void HandleGet(HttpContext httpContext)
        {
            string filename;
            if (!TryGetFilename(httpContext, out filename))
            {
                httpContext.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            if (File.Exists(filename))
            {
                httpContext.Response.Body = LoadFile(filename);
            }
            else
            {
                httpContext.Response.StatusCode = HttpStatusCode.NotFound;
            }
        }

        private void HandlePost(HttpContext httpContext)
        {
            string filename;
            if (!TryGetFilename(httpContext, out filename))
            {
                httpContext.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            var path = Path.GetDirectoryName(filename);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            File.WriteAllBytes(filename, httpContext.Request.Body ?? new byte[0]);
        }

        private BinaryBody LoadFile(string filename)
        {
            var mimeType = MimeTypeProvider.GetMimeTypeFromFile(filename);
            var fileContent = File.ReadAllBytes(filename);

            return new BinaryBody { Content = fileContent, MimeType = mimeType };
        }

        private bool TryGetFilename(HttpContext httpContext, out string filename)
        {
            filename = null;

            var relativeUrl = Uri.UnescapeDataString(httpContext.Request.Uri);
            relativeUrl = relativeUrl.TrimStart('/');

            var urlAffectsDifferentController = !relativeUrl.StartsWith(_name, StringComparison.OrdinalIgnoreCase);
            if (urlAffectsDifferentController)
            {
                return false;
            }

            if (relativeUrl.EndsWith("/"))
            {
                relativeUrl += DefaultFile;
            }

            relativeUrl = relativeUrl.Substring(_name.Length).Trim('/');
            relativeUrl = relativeUrl.Replace("/", @"\");

            filename = Path.Combine(_rootDirectory, relativeUrl);
            return true;
        }
    }
}
