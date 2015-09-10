using System.Text;
using Windows.Data.Json;

namespace CK.HomeAutomation.Networking
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public StringBuilder Body { get; } = new StringBuilder();
        public JsonObject Result { get; set; }
        public string MimeType { get; set; }
    }
}