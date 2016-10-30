using Newtonsoft.Json.Linq;

namespace HA4IoT.Features
{
    public interface IFeature
    {
        void Invoke(JToken parameters);
    }
}
