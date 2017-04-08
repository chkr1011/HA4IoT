using System.Collections.Generic;
using HA4IoT.Contracts.Services.Resources;

namespace HA4IoT.Services.Resources
{
    public class SetTextsRequest
    {
        public List<Resource> Resources { get; set; }
    }
}
