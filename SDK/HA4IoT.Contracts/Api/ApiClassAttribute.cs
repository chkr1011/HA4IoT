using System;

namespace HA4IoT.Contracts.Api
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ApiClassAttribute : Attribute
    {
        public ApiClassAttribute(string @namespace)
        {
            Namespace = @namespace;
        }

        protected ApiClassAttribute()
        {
        }

        public string Namespace { get; protected set; }
    }
}
