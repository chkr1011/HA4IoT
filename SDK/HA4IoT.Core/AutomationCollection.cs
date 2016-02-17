using HA4IoT.Contracts.Automations;

namespace HA4IoT.Core
{
    public class AutomationCollection : GenericControllerCollection<AutomationId, IAutomation>
    {
        ////private readonly Dictionary<AutomationId, IAutomation> _automations = new Dictionary<AutomationId, IAutomation>();

        ////public void AddUnique(IAutomation automation)
        ////{
        ////    if (automation == null) throw new ArgumentNullException(nameof(automation));

        ////    _automations.Add(automation.Id, automation);
        ////}

        ////public void Add(IAutomation automation)
        ////{
        ////    if (automation == null) throw new ArgumentNullException(nameof(automation));

        ////    _automations[automation.Id] = automation;
        ////}

        ////public TAutomation Get<TAutomation>(AutomationId id) where TAutomation : IAutomation
        ////{
        ////    if (id == null) throw new ArgumentNullException(nameof(id));

        ////    IAutomation automation;
        ////    if (!_automations.TryGetValue(id, out automation))
        ////    {
        ////        throw new InvalidOperationException("Automation with ID '" + id + "' is not registered.");
        ////    }

        ////    return (TAutomation)automation;
        ////}

        ////public IList<TAutomation> GetAll<TAutomation>() where TAutomation : IAutomation
        ////{
        ////    return _automations.Values.OfType<TAutomation>().ToList();
        ////}

        ////public IList<IAutomation> GetAll()
        ////{
        ////    return _automations.Values.ToList();
        ////}
    }
}
