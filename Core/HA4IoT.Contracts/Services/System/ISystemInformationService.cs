using System;

namespace HA4IoT.Contracts.Services.System
{
    public interface ISystemInformationService : IService
    {
        void Set(string name, object value);

        void Set(string name, Func<object> valueProvider);
    }
}
