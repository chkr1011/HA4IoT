using System;
using HA4IoT.Contracts.Configuration;

namespace HA4IoT.Actuators.Automations
{
    public static class AutomaticTurnOnAndOffAutomationExtensions
    {
        public static AutomaticTurnOnAndOffAutomation SetupAutomaticTurnOnAndOffAutomation(this IRoom room)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return new AutomaticTurnOnAndOffAutomation(room.Controller.Timer);
        }
    }
}
