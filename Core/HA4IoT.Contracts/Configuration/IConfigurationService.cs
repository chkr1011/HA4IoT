using HA4IoT.Contracts.Services;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Configuration
{
    public interface IConfigurationService : IService
    {
        JObject GetSection(string name);

        TSection GetConfiguration<TSection>(string name) where TSection : class;
    }
}
