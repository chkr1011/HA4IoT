using System;

namespace HA4IoT.Contracts.Api.Cloud
{
    public class CloudMessageHeader
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public DateTime Created { get; } = DateTime.UtcNow;
    }
}
