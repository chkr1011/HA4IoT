using System;
using System.Xml.Linq;

namespace HA4IoT.Contracts.Configuration
{
    public class ConfigurationInvalidException : Exception
    {
        public ConfigurationInvalidException(string message, XElement affectedElement) : base(message)
        {
            AffectedElement = affectedElement;
        }

        public XElement AffectedElement { get; private set; }
    }
}
