using System;
using System.Reflection;

namespace HA4IoT.Contracts.Services.Settings
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingsUriAttribute : Attribute
    {
        public SettingsUriAttribute(Type type)
        {
            if (typeof(IService).IsAssignableFrom(type))
            {
                Uri = "Service/" + type.Name;
            }
            else
            {
                Uri = type.Name;
            }
        }

        public string Uri { get; }
    }
}
