using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace HA4IoT.TraceViewer
{
    public class TraceItemsParser
    {
        public IList<TraceItem> Parse(string source)
        {
            JObject package = JObject.Parse(source);
            string type = package.Property("type").Value.ToString();
            int version = (int)package.Property("version").Value;

            var traceItems = new List<TraceItem>();
            foreach (var notification in package.Property("notifications").Value)
            {
                var item = notification.ToObject<JObject>();

                DateTime timestamp = DateTime.Parse(item.Property("Timestamp").Value.ToString());
                int threadId = int.Parse(item.Property("ThreadId").Value.ToString());
                string severity = item.Property("Severity").Value.ToString();
                string message = item.Property("Message").Value.ToString();

                var typeValue = (TraceItemSeverity)Enum.Parse(typeof(TraceItemSeverity), severity);
                traceItems.Add(new TraceItem(timestamp, threadId, typeValue, message));
            }

            return traceItems;
        }
    }
}
