using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(HA4IoT.CloudApi.Startup))]

namespace HA4IoT.CloudApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
