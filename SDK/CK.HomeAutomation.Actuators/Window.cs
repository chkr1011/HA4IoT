using System.Collections.Generic;
using Windows.Data.Json;
using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public class Window : ActuatorBase
    {
        private readonly List<Casement> _casements = new List<Casement>(); 

        public Window(string id, IHttpRequestController httpApiController, INotificationHandler notificationHandler) : base(id, httpApiController, notificationHandler)
        {
        }

        public Window WithCasement(Casement casement)
        {
            _casements.Add(casement);
            return this;
        }

        public Window WithCasement(IBinaryInput fullOpenReedSwitch, IBinaryInput tiltReedSwitch = null)
        {
            return WithCasement(new Casement(fullOpenReedSwitch, tiltReedSwitch));
        }

        public override void ApiGet(ApiRequestContext context)
        {
            var state = new JsonArray();
            foreach (var casement in _casements)
            {
                state.Add(JsonValue.CreateStringValue(casement.State.ToString()));
            }            

            context.Response.SetNamedValue("state", state);
        }
    }
}
