using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Api
{
    public static class ExceptionSerializer
    {
        public static JObject SerializeException(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            // Do not use a generic serializer because sometines not all propterties are readable
            // and throwing exceptions in the getter.
            var json = new JObject
            {
                ["ExceptionType"] = exception.GetType().FullName,
                ["ExceptionSource"] = exception.Source,
                ["ExceptionMessage"] = exception.Message,
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
