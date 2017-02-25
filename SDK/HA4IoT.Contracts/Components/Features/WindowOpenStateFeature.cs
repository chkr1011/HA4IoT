using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components.Features
{
    public class WindowOpenStateFeature : IComponentFeature
    {
        public JToken Serialize()
        {
            return JToken.FromObject(null);
        }
    }
}
