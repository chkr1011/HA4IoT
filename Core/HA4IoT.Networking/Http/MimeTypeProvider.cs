using System.IO;

namespace HA4IoT.Networking.Http
{
    public static class MimeTypeProvider
    {
        public const string Csv = "text/csv; charset=utf-8";
        public const string Html = "text/html; charset=utf-8";
        public const string Javascript = "text/javascript; charset=utf-8";
        public const string Json = "application/json; charset=utf-8";
        public const string Css = "text/css; charset=utf-8";
        public const string Png = "image/png";
        public const string Jpg = "image/jpg";
        public const string Manifest = "text/cache-manifest; charset=utf-8";
        public const string PlainText = "text/plain; charset=utf-8";
        public const string OctetStream = "application/octet-stream";

        public static string GetMimeTypeFromFile(string filename)
        {
            string extension = Path.GetExtension(filename).ToLower();
            switch (extension)
            {
                case ".csv":
                    {
                        return Csv;
                    }

                case ".html":
                case ".htm":
                    {
                        return Html;
                    }

                case ".js":
                    {
                        return Javascript;
                    }

                case ".json":
                    {
                        return Json;
                    }

                case ".css":
                    {
                        return Css;
                    }

                case ".png":
                    {
                        return Png;
                    }

                case ".jpeg":
                case ".jpg":
                    {
                        return Jpg;
                    }

                case ".bin":
                    {
                        return OctetStream;
                    }

                case ".manifest":
                    {
                        return Manifest;
                    }

                default:
                    {
                        return PlainText;
                    }
            }
        }
    }
}
