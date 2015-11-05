using System;

namespace HA4IoT.Networking
{
    public class HttpHeader
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public static HttpHeader Create()
        {
            return new HttpHeader();
        }

        public HttpHeader WithName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            Name = name;
            return this;
        }

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
