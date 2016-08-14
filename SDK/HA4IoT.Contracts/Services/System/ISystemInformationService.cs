using System;

namespace HA4IoT.Contracts.Services.System
{
    public interface ISystemInformationService : IApiExposedService
    {
        void Set(string name, string value);

        void Set(string name, int? value);

        void Set(string name, float? value);

        void Set(string name, TimeSpan? value);

        void Set(string name, DateTime? value);
    }
}
