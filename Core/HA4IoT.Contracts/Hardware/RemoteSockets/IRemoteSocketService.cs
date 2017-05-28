using HA4IoT.Contracts.Hardware.RemoteSockets.Codes;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Hardware.RemoteSockets
{
    public interface IRemoteSocketService : IService
    {
        IBinaryOutput RegisterRemoteSocket(string id, string adapterDeviceId, Lpd433MhzCodePair codePair);
        IBinaryOutput GetRemoteSocket(string id);
        
        void SendCode(string adapterDeviceId, Lpd433MhzCode code);
        void RegisterRemoteSockets();
    }
}