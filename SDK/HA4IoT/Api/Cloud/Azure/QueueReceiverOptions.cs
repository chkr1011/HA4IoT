using System;

namespace HA4IoT.Api.Cloud.Azure
{
    public class QueueReceiverOptions
    {
        public string NamespaceName { get; set; }

        public string QueueName { get; set; }

        public string Authorization { get; set; }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
    }
}
