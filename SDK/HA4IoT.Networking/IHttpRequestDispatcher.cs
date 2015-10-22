namespace CK.HomeAutomation.Networking
{
    public interface IHttpRequestDispatcher
    {
        IHttpRequestController GetController(string name);
    
        void MapFolder(string controllerName, string rootDirectory);
    }
}
