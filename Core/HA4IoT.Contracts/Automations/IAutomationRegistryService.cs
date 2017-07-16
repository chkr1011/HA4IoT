using HA4IoT.Contracts.Services;
using System.Collections.Generic;

namespace HA4IoT.Contracts.Automations
{
    public interface IAutomationRegistryService : IService
    {
        void AddAutomation(IAutomation automation);

        IList<TAutomation> GetAutomations<TAutomation>() where TAutomation : IAutomation;

        TAutomation GetAutomation<TAutomation>(string id) where TAutomation : IAutomation;

        IList<IAutomation> GetAutomations();
    }
}
