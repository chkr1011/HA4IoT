using System;
using System.Text;
using MQTTnet.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Devices
{
    internal static class MessageInterceptor
    {
        public static void Intercept(MqttApplicationMessageInterceptorContext context)
        {
            if (context.ApplicationMessage.Payload == null || context.ApplicationMessage.Payload.Length == 0)
            {
                return;
            }

            try
            {
                var content = Encoding.UTF8.GetString(context.ApplicationMessage.Payload);
                if (!content.StartsWith("{") || !content.EndsWith("}"))
                {
                    return;
                }

                var json = JObject.Parse(content);
                var timestampProperty = json.Property("timestamp");
                if (timestampProperty != null && timestampProperty.Value.Type == JTokenType.Null)
                {
                    timestampProperty.Value = DateTime.Now.ToString("O");
                    context.ApplicationMessage.Payload = Encoding.UTF8.GetBytes(json.ToString(Formatting.None));
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}
