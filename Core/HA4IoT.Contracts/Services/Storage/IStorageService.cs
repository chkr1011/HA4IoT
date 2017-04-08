namespace HA4IoT.Contracts.Services.Storage
{
    public interface IStorageService : IService
    {
        bool TryRead<TData>(string filename, out TData data);

        void Write<TData>(string filename, TData content);
    }
}
