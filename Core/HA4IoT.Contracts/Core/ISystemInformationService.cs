using System;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Core
{
    public interface ISystemInformationService : IService
    {
        void Set(string key, object value);

        void Set(string key, Func<object> valueProvider);

        void Delete(string key);
    }
}
