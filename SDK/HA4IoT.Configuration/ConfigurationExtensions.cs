using System;
using System.Xml.Linq;
using HA4IoT.Contracts.Configuration;

namespace HA4IoT.Configuration
{
    public static class ConfigurationExtensions
    {
        public static bool GetBoolFromAttribute(this XElement element, string attributeName, bool defaultValue)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (attributeName == null) throw new ArgumentNullException(nameof(attributeName));

            return Convert.ToBoolean(element.GetValueFromAttribute(attributeName, defaultValue.ToString()));
        }

        public static bool GetMandatoryBoolFromAttribute(this XElement element, string attributeName)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (attributeName == null) throw new ArgumentNullException(nameof(attributeName));

            return Convert.ToBoolean(element.GetMandatoryValueFromAttribute(attributeName));
        }

        public static int GetIntFromAttribute(this XElement element, string attributeName, int defaultValue)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (attributeName == null) throw new ArgumentNullException(nameof(attributeName));

            return Convert.ToInt32(element.GetValueFromAttribute(attributeName, defaultValue.ToString()));
        }

        public static int GetMandatoryIntFromAttribute(this XElement element, string attributeName)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (attributeName == null) throw new ArgumentNullException(nameof(attributeName));

            return Convert.ToInt32(element.GetMandatoryValueFromAttribute(attributeName));
        }

        public static string GetValueFromAttribute(this XElement element, string attributeName, string defaultValue)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (attributeName == null) throw new ArgumentNullException(nameof(attributeName));

            var attribute = element.Attribute(attributeName);
            if (attribute == null)
            {
                return defaultValue;
            }

            return attribute.Value;
        }

        public static string GetMandatoryValueFromAttribute(this XElement element, string attributeName)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (attributeName == null) throw new ArgumentNullException(nameof(attributeName));

            var attribute = element.Attribute(attributeName);
            if (attributeName == null)
            {
                throw new ConfigurationInvalidException("Mandatory attribute '" + attributeName + "' missing.", element);
            }

            return attribute.Value;
        }
    }
}
