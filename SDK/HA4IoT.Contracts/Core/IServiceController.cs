using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Core
{
    public interface IServiceController
    {
        void RegisterService(IService service);

        TService GetService<TService>() where TService : IService;

        bool TryGetService<TService>(out TService service) where TService : IService;
    }
}
