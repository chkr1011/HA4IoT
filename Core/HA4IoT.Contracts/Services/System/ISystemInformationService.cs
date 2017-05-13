using System;

namespace HA4IoT.Contracts.Services.System
{
    public interface ISystemInformationService : IService
    {
        void Set(string key, object value);

        void Set(string key, Func<object> valueProvider);

        void Delete(string key);
    }
}
