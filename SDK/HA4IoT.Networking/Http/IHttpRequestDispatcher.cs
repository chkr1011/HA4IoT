using HA4IoT.Contracts.Networking.Http;

namespace HA4IoT.Networking.Http
{
    public interface IHttpRequestDispatcher
    {
        IHttpRequestController GetController(string name);
    
        void MapFolder(string controllerName, string rootDirectory);
    }
}
