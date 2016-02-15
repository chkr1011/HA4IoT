using System;
using HA4IoT.Contracts.Configuration;

namespace HA4IoT.Actuators.Automations
{
    public static class AutomaticConditionalOnAutomationExtensions
    {
        public static AutomaticConditionalOnAutomation SetupAutomaticConditionalOnAutomation(this IArea room)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return new AutomaticConditionalOnAutomation(room.Controller.Timer);
        }
    }
}
