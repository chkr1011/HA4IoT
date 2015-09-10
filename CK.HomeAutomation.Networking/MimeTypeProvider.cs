using System.IO;

namespace CK.HomeAutomation.Networking
{
    public class MimeTypeProvider
    {
        public string GetMimeTypeOfFile(string filename)
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

                case ".css":
                    {
                        return "text/css; charset=utf-8";
                    }

                default:
                    {
                        return "text/plain; charset=utf-8";
                    }
            }
        }
    }
}
