namespace CK.HomeAutomation.Networking
{
    public interface IHttpRequestDispatcher
    {
        IHttpRequestController GetController(string name);
    
        void MapDirectory(string controllerName, string rootDirectory);
    }
}
