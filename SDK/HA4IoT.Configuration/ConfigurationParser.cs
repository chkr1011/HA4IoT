using System;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Configuration
{
    public class ConfigurationParser
    {
        private readonly IController _controller;

        public ConfigurationParser(IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            _controller = controller;
        }


    }
}
