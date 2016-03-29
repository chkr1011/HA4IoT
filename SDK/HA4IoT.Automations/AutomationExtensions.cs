using System;
using HA4IoT.Contracts.Automations;

namespace HA4IoT.Automations
{
    public static class AutomationExtensions
    {
        // TODO: Add const for setting name.
        public static void Enable(this IAutomation automation)
        {
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            automation.Settings.SetValue("IsEnabled", true);
        }

        public static void Disable(this IAutomation automation)
        {
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            automation.Settings.SetValue("IsEnabled", false);
        }

        public static bool GetIsEnabled(this IAutomation automation)
        {
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            return automation.Settings.GetBoolean("IsEnabled");
        }
    }
}
