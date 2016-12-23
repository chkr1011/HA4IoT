using System;
using System.Reflection;
using HA4IoT.Contracts.Api;

namespace HA4IoT.Contracts.Services
{
    public class ApiServiceClassAttribute : ApiClassAttribute
    {
        public ApiServiceClassAttribute(Type interfaceType)
        {
            if (interfaceType == null) throw new ArgumentNullException(nameof(interfaceType));

            if (!typeof(IService).IsAssignableFrom(interfaceType))
            {
                throw new ArgumentException($"Type {interfaceType.FullName} is not implementing IService.");
            }
            
            Uri = "Service/" + interfaceType.Name;
        }
    }
}
