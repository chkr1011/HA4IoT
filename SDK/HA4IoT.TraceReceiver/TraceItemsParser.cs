using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.TraceReceiver
{
    public class TraceItemsParser
    {
        public IList<TraceItem> Parse(string source)
        {
            JObject data = ParsePackage(source);
            
            string type = (string) data["Type"];
            int version = (int) data["Version"];

            var traceItems = new List<TraceItem>();
            foreach (var traceItem in data["TraceItems"])
            {
                var item = traceItem.ToObject<JObject>();

                var id = (long) item["Id"];
                var threadId = (int) item["ThreadId"];
                var severity = (string) item["Severity"];
                var message = (string) item["Message"];

                string timestampValue = (string) item["Timestamp"];
                DateTime timestamp = DateTime.ParseExact(timestampValue, "o", CultureInfo.InvariantCulture);

                var typeValue = (TraceItemSeverity)Enum.Parse(typeof(TraceItemSeverity), severity);
                traceItems.Add(new TraceItem(id, timestamp, threadId, typeValue, message));
            }

            return traceItems;
        }

        public JObject ParsePackage(string source)
        {
            using (var stringReader = new StringReader(source))
            {
                var jsonTextReader = new JsonTextReader(stringReader);

                var serializer = new JsonSerializer();
                serializer.DateParseHandling = DateParseHandling.None;

                return serializer.Deserialize<JObject>(jsonTextReader);
            }
        }
    }
}
