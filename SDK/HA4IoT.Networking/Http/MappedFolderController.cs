using System;
using System.IO;
using System.Text;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Networking.Http
{
    public class MappedFolderController : HttpRequestController
    {
        private readonly MimeTypeProvider _mimeTypeProvider = new MimeTypeProvider();
        private readonly string _name;
        private readonly string _rootFolder;

        public MappedFolderController(string name, string rootFolder, HttpServer httpServer) : base(name, httpServer)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (rootFolder == null) throw new ArgumentNullException(nameof(rootFolder));

            _name = name;
            _rootFolder = rootFolder;

            Handle(HttpMethod.Get, string.Empty).WithAnySubUrl().Using(HandleGet);
            Handle(HttpMethod.Post, string.Empty).WithAnySubUrl().Using(HandlePost);
        }

        public string DefaultFile { get; } = "index.html";

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
                LoadFile(filename, httpContext);
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

            string path = Path.GetDirectoryName(filename);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            File.WriteAllText(filename, httpContext.Request.Body, Encoding.UTF8);
        }

        private void LoadFile(string filename, HttpContext httpContext)
        {
            byte[] fileContent = File.ReadAllBytes(filename);
            string mimeType = _mimeTypeProvider.GetMimeTypeFromFile(filename);

            httpContext.Response.Body = new BinaryBody(fileContent).WithMimeType(mimeType);
        }

        private bool TryGetFilename(HttpContext httpContext, out string filename)
        {
            filename = null;

            string relativeUrl = Uri.UnescapeDataString(httpContext.Request.Uri);
            relativeUrl = relativeUrl.Trim('/');

            bool urlAffectsDifferentController = !relativeUrl.StartsWith(_name, StringComparison.OrdinalIgnoreCase);
            if (urlAffectsDifferentController)
            {
                return false;
            }
            
            bool urlContainsNoFile = relativeUrl.Equals(_name, StringComparison.OrdinalIgnoreCase);
            if (urlContainsNoFile)
            {
                relativeUrl += "/" + DefaultFile;
            }

            relativeUrl = relativeUrl.Substring(_name.Length).Trim('/');
            relativeUrl = relativeUrl.Replace("/", @"\");

            filename = Path.Combine(_rootFolder, relativeUrl);
            return true;
        }
    }
}
