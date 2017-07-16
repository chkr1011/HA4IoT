using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Resources
{
    public interface IResourceService : IService
    {
        void RegisterType(Enum @enum);

        void RegisterText(Enum id, string value);

        string GetText(Enum id);

        string GetText(Enum id, params object[] formatParameterObjects);

        string GetText(Enum id, IDictionary<string, object> formatParameters);
    }
}
