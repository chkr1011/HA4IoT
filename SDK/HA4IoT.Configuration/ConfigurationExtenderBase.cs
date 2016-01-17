using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Configuration
{
    public abstract class ConfigurationExtenderBase
    {
        protected ConfigurationExtenderBase(ConfigurationParser parser, IController controller)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            Parser = parser;
            Controller = controller;
        }

        public string Namespace { get; protected set; } = string.Empty;

        protected ConfigurationParser Parser { get; }

        protected IController Controller { get; }
    }
}
