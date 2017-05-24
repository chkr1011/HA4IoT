using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Core
{
    [ApiServiceClass(typeof(IDateTimeService))]
    public class DateTimeService : ServiceBase, IDateTimeService
    {
        public DateTimeService(IScriptingService scriptingService)
        {
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));

            scriptingService.RegisterScriptProxy(s => new DateTimeScriptProxy(this));
        }

        public DateTime Date => DateTime.Now.Date;

        public TimeSpan Time => DateTime.Now.TimeOfDay;
    
        public DateTime Now => DateTime.Now;
    
        [ApiMethod]
        public void Status(IApiCall apiCall)
        {
            apiCall.Result = JObject.FromObject(this);
        }
    }
}
