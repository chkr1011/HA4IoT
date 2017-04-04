using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Networking.Json
{
    public static class JsonSerializer
    {
        public static JObject SerializeException(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            // Do not use a generic serializer because sometines not all propterties are readable
            // and throwing exceptions in the getter.
            var json = new JObject
            {
                ["Type"] = exception.GetType().FullName,
                ["Source"] = exception.Source,
                ["Message"] = exception.Message,
                ["StackTrace"] = exception.StackTrace
            };

            if (exception.InnerException != null)
            {
                json["InnerException"] = SerializeException(exception.InnerException);
            }
            else
            {
                json["InnerException"] = null;
            }

            return json;
        }
    }
}
