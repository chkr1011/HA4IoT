using CK.HomeAutomation.Actuators.Conditions;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace CK.HomeAutomation.Actuators.Tests
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
