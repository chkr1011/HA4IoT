using HA4IoT.Contracts.Hardware.RemoteSockets.Codes;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Hardware.RemoteSockets
{
    public interface IRemoteSocketService : IService
    {
        IBinaryOutput GetRemoteSocket(string id);
        
        void SendCode(string adapterDeviceId, Lpd433MhzCode code);
        void RegisterRemoteSockets();
    }
}