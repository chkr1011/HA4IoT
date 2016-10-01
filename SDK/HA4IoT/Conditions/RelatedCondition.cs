using System;
using HA4IoT.Contracts.Conditions;

namespace HA4IoT.Conditions
{
    public class RelatedCondition
    {
        public ConditionRelation Relation { get; private set; } = ConditionRelation.And;

        public Condition Condition { get; private set; }

        public RelatedCondition WithCondition(Condition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            Condition = condition;
            return this;
        }

        public RelatedCondition WithRelation(ConditionRelation relation)
        {
            Relation = relation;
            return this;
        }

        public ConditionState Validate()
        {
            if (Condition == null)
            {
                throw new InvalidOperationException("Related condition has no condition.");
            }

            return Condition.Validate();
        }
    }
}
