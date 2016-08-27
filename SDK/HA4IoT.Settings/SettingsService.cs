using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Settings
{
    [ApiServiceClass(typeof(ISettingsService))]
    public class SettingsService : ServiceBase, ISettingsService
    {

    }
}
