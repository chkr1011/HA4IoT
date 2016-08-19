using System;

namespace HA4IoT.Contracts.Api
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ApiClassAttribute : Attribute
    {
        public ApiClassAttribute(string uri)
        {
            Uri = uri;
        }

        protected ApiClassAttribute()
        {
        }

        public string Uri { get; protected set; }
    }
}
