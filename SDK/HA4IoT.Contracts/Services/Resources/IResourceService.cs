using System;
using System.Collections.Generic;

namespace HA4IoT.Contracts.Services.Resources
{
    public interface IResourceService : IService
    {
        string GetText(Enum id);

        string GetText(Enum id, IDictionary<string, object> formatParameters);

        string GetText(Enum id, params object[] formatParameterObjects);
    }
}
