using HA4IoT.Contracts.Scripting;

namespace HA4IoT.Contracts.Services
{
    public abstract class ServiceBase : IService
    {
        public virtual void Startup()
        {
        }

        public virtual IScriptProxy CreateScriptProxy(IScriptingSession scriptingSession, bool x)
        {
            return null;
        }
    }
}
