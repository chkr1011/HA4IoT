using System;

namespace HA4IoT.Contracts.Api
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ApiMethodAttribute : Attribute
    {
        public ApiMethodAttribute(ApiCallType callType)
        {
            CallType = callType;
        }

        public ApiCallType CallType { get; }
    }
}
