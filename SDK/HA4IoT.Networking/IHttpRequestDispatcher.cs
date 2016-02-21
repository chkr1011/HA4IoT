using HA4IoT.Contracts.Networking;

namespace HA4IoT.Networking
{
    public interface IHttpRequestDispatcher
    {
        IHttpRequestController GetController(string name);
    
        void MapFolder(string controllerName, string rootDirectory);
    }
}
