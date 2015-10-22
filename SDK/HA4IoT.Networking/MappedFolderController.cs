using System;
using System.IO;

namespace HA4IoT.Networking
{
    public class MappedFolderController : HttpRequestController
    {
        private readonly MimeTypeProvider _mimeTypeProvider = new MimeTypeProvider();

        public MappedFolderController(string name, string rootFolder, HttpServer httpServer) : base(name, httpServer)
        {
            Handle(HttpMethod.Get, string.Empty).WithAnySubUrl().Using(c =>
            {
                string relativeUrl = c.Request.Uri.Trim('/');
                int index = relativeUrl.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                if (index == -1)
                {
                    return;
                }

                relativeUrl = relativeUrl.Substring(index + name.Length).Replace("/", "\\");
                relativeUrl = Uri.UnescapeDataString(relativeUrl);
                string filename = rootFolder + relativeUrl;

                if (File.Exists(filename))
                {

                    byte[] fileContent = File.ReadAllBytes(filename);
                    string mimeType = _mimeTypeProvider.GetMimeTypeFromFile(filename);

                    c.Response.Body = new BinaryBody(fileContent).WithMimeType(mimeType);
                }
            });
        }
    }
}
