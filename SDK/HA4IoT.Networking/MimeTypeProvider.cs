using System.IO;

namespace HA4IoT.Networking
{
    public class MimeTypeProvider
    {
        public string GetMimeTypeFromFile(string filename)
        {
            string extension = Path.GetExtension(filename).ToLower();
            switch (extension)
            {
                case ".html":
                case ".htm":
                    {
                        return "text/html; charset=utf-8";
                    }

                case ".js":
                    {
                        return "text/javascript; charset=utf-8";
                    }

                case ".json":
                    {
                        return "application/json; charset=utf-8";
                    }

                case ".css":
                    {
                        return "text/css; charset=utf-8";
                    }

                case ".png":
                    {
                        return "image/png";
                    }

                case ".jpeg":
                case ".jpg":
                    {
                        return "image/jpg";
                    }

                case ".manifest":
                    {
                        return "text/cache-manifest";
                    }

                default:
                    {
                        return "text/plain; charset=utf-8";
                    }
            }
        }
    }
}
