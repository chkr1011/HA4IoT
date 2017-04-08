using FluentAssertions;
using HA4IoT.Conditions;
using HA4IoT.Contracts.Conditions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Actuators
{
    [TestClass]
    public class ConditionsValidatorTests
    {
        [TestMethod]
        public void ShouldBeNotFulfilled_WithNotFulfilledDefault_AndOneFulfilledCondiiton()
        {
            var conditionsValidator = new ConditionsValidator()
                .WithCondition(ConditionRelation.Or, new NotFulfilledTestCondition())
                .WithCondition(ConditionRelation.Or, new FulfilledTestCondition())
                .WithDefaultState(ConditionState.NotFulfilled);

            conditionsValidator.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }
    }
}
