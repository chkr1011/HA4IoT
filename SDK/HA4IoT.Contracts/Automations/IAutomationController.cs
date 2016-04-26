using System.Collections.Generic;

namespace HA4IoT.Contracts.Automations
{
    public interface IAutomationController
    {
        void AddAutomation(IAutomation automation);

        IList<TAutomation> GetAutomations<TAutomation>() where TAutomation : IAutomation;

        TAutomation GetAutomation<TAutomation>(AutomationId id) where TAutomation : IAutomation;

        IList<IAutomation> GetAutomations();
    }
}
