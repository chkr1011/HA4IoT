using System.Collections.Generic;

namespace HA4IoT.Contracts.Automations
{
    public interface IAutomationController
    {
        void AddAutomation(IAutomation automation);

        IList<TAutomation> Automations<TAutomation>() where TAutomation : IAutomation;

        TAutomation Automation<TAutomation>(AutomationId id) where TAutomation : IAutomation;

        IList<IAutomation> Automations();
    }
}
