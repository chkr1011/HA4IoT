using System;

namespace HA4IoT.Networking
{
    public class HttpHeader
    {
        public static HttpHeader Create(string name)
        {
            return new HttpHeader(name);
        }

        public HttpHeader(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public string Value { get; set; }

        public HttpHeader WithValue(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            Value = value;
            return this;
        }

        public override string ToString()
        {
            return Name + ":" + Value;
        }
    }
}
