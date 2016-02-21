using System;
using System.IO;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Networking
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

            Handle(HttpMethod.Get, string.Empty).WithAnySubUrl().Using(HandleRequest);
        }

        public string DefaultFile { get; } = "index.html";

        private void HandleRequest(HttpContext httpContext)
        {
            string relativeUrl = Uri.UnescapeDataString(httpContext.Request.Uri);
            relativeUrl = relativeUrl.Trim('/');

            bool urlAffectsDifferentController = !relativeUrl.StartsWith(_name, StringComparison.OrdinalIgnoreCase);
            if (urlAffectsDifferentController)
            {
                return;
            }

            bool urlContainsNoFile = relativeUrl.Equals(_name, StringComparison.OrdinalIgnoreCase);
            if (urlContainsNoFile)
            {
                relativeUrl += "/" + DefaultFile;
            }

            relativeUrl = relativeUrl.Substring(_name.Length).Trim('/');
            relativeUrl = relativeUrl.Replace("/", @"\");

            string filename = Path.Combine(_rootFolder, relativeUrl);
            if (File.Exists(filename))
            {
                LoadFile(filename, httpContext);
            }
            else
            {
                httpContext.Response.StatusCode = HttpStatusCode.NotFound;
            }
        }

        private void LoadFile(string filename, HttpContext httpContext)
        {
            byte[] fileContent = File.ReadAllBytes(filename);
            string mimeType = _mimeTypeProvider.GetMimeTypeFromFile(filename);

            httpContext.Response.Body = new BinaryBody(fileContent).WithMimeType(mimeType);
        }
    }
}
