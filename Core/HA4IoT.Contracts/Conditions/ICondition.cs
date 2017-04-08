namespace HA4IoT.Contracts.Conditions
{
    public interface ICondition
    {
        ConditionState Validate();
    }
}
