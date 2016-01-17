using System;
using System.Linq;
using System.Xml.Linq;
using HA4IoT.Contracts.Configuration;

namespace HA4IoT.Configuration
{
    public static class ConfigurationExtensions
    {
        public static TValue GetGenericValueFromAttribute<TValue>(this XElement element, string attributeName, TValue defaultValue, Func<string, TValue> converter)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (attributeName == null) throw new ArgumentNullException(nameof(attributeName));
            if (converter == null) throw new ArgumentNullException(nameof(converter));

            var attribute = element.Attribute(attributeName);
            if (attribute == null)
            {
                return defaultValue;
            }

            return converter(attribute.Value);
        }

        public static TValue GetMandatoryGenericValueFromAttribute<TValue>(this XElement element, string attributeName, Func<string, TValue> converter)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (attributeName == null) throw new ArgumentNullException(nameof(attributeName));
            if (converter == null) throw new ArgumentNullException(nameof(converter));

            var attribute = element.Attribute(attributeName);
            if (attribute == null)
            {
                throw new ConfigurationInvalidException("Mandatory attribute '" + attributeName + "' missing.", element);
            }
            
            return converter(attribute.Value);
        }

        public static string GetStringFromAttribute(this XElement element, string attributeName, string defaultValue)
        {
            return GetGenericValueFromAttribute(element, attributeName, defaultValue, v => v);
        }

        public static bool GetBoolFromAttribute(this XElement element, string attributeName, bool defaultValue)
        {
            return GetGenericValueFromAttribute(element, attributeName, defaultValue, Convert.ToBoolean);
        }

        public static int GetIntFromAttribute(this XElement element, string attributeName, int defaultValue)
        {
            return GetGenericValueFromAttribute(element, attributeName, defaultValue, Convert.ToInt32);
        }

        public static double GetDoubleFromAttribute(this XElement element, string attributeName, double defaultValue)
        {
            return GetGenericValueFromAttribute(element, attributeName, defaultValue, Convert.ToDouble);
        }

        public static TimeSpan GetTimeSpanFromAttribute(this XElement element, string attributeName, TimeSpan defaultValue)
        {
            return GetGenericValueFromAttribute(element, attributeName, defaultValue, TimeSpan.Parse);
        }

        public static string GetMandatoryStringFromAttribute(this XElement element, string attributeName)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (attributeName == null) throw new ArgumentNullException(nameof(attributeName));

            return element.GetMandatoryGenericValueFromAttribute(attributeName, v => v);
        }

        public static bool GetMandatoryBoolFromAttribute(this XElement element, string attributeName)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (attributeName == null) throw new ArgumentNullException(nameof(attributeName));

            return element.GetMandatoryGenericValueFromAttribute(attributeName, Convert.ToBoolean);
        }

        public static int GetMandatoryIntFromAttribute(this XElement element, string attributeName)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (attributeName == null) throw new ArgumentNullException(nameof(attributeName));

            return element.GetMandatoryGenericValueFromAttribute(attributeName, Convert.ToInt32);
        }

        public static double GetMandatoryDoubleFromAttribute(this XElement element, string attributeName)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (attributeName == null) throw new ArgumentNullException(nameof(attributeName));

            return element.GetMandatoryGenericValueFromAttribute(attributeName, Convert.ToDouble);
        }

        public static XElement GetMandatorySingleChildFromContainer(this XElement element, string containerElementName)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (containerElementName == null) throw new ArgumentNullException(nameof(containerElementName));

            var containerElement = element.Element(containerElementName);
            if (containerElement == null)
            {
                throw new ConfigurationInvalidException("Mandatory container element '" + containerElementName + "' missing.", element);
            }

            var childElements = containerElement.Elements().ToList();

            if (childElements.Count == 0)
            {
                throw new ConfigurationInvalidException("Mandatory container element '" + containerElementName + "' is empty.", element);
            }

            if (childElements.Count > 1)
            {
                throw new ConfigurationInvalidException("Mandatory container element '" + containerElementName + "' contains more than one element.", element);
            }

            return childElements.First();
        }

        public static XElement GetMandatorySingleChildElementOrFromContainer(this XElement element, string containerElementName)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (containerElementName == null) throw new ArgumentNullException(nameof(containerElementName));

            var childElements = element.Elements().ToList();
            if (childElements.Count() == 1)
            {
                return childElements.First();
            }

            return element.GetMandatorySingleChildFromContainer(containerElementName);
        }

        public static bool HasChildElement(this XElement element, string childElementName)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (childElementName == null) throw new ArgumentNullException(nameof(childElementName));

            return element.Element(childElementName) != null;
        }
    }
}
