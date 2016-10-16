using FluentAssertions;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Conditions;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Tests.Actuators
{
    [TestClass]
    public class ConditionTests
    {
        [TestMethod]
        public void ConditionWithTrueExpression_ShouldBeFulfilled()
        {
            var condition = new Condition().WithExpression(() => true);
            condition.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }

        [TestMethod]
        public void ConditionWithFulfilledExpression_ShouldBeFulfilled()
        {
            var condition = new Condition().WithExpression(() => ConditionState.Fulfilled);
            condition.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }

        [TestMethod]
        public void ConditionWithFalseExpression_ShouldBeNotFulfilled()
        {
            var condition = new Condition().WithExpression(() => false);
            condition.Validate().ShouldBeEquivalentTo(ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void ConditionWithNotfulfilledExpression_ShouldBeNotFulfilled()
        {
            var condition = new Condition().WithExpression(() => ConditionState.NotFulfilled);
            condition.Validate().ShouldBeEquivalentTo(ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void FulfilledCondition_AND_FulfilledCondition_ShouldBeFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.And, new FulfilledTestCondition());
            condition.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }

        [TestMethod]
        public void FulfilledCondition_AND_NotFulfilledCondition_ShouldBeNotFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.And,  new NotFulfilledTestCondition());
            condition.Validate().ShouldBeEquivalentTo(ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void NotFulfilledCondition_AND_FulfilledCondition_ShouldBeNotFulfilled()
        {
            var condition = new NotFulfilledTestCondition().WithRelatedCondition(ConditionRelation.And, new FulfilledTestCondition());
            condition.Validate().ShouldBeEquivalentTo(ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void NotFulfilledCondition_OR_FulfilledCondition_ShouldBeFulfilled()
        {
            var condition = new NotFulfilledTestCondition().WithRelatedCondition(ConditionRelation.Or, new FulfilledTestCondition());
            condition.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }

        [TestMethod]
        public void FulfilledCondition_OR_NotFulfilledCondition_ShouldBeFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.Or, new NotFulfilledTestCondition());
            condition.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }

        [TestMethod]
        public void FulfilledCondition_ANDNOT_NotFulfilledCondition_ShouldBeFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.AndNot, new NotFulfilledTestCondition());
            condition.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }
        
        [TestMethod]
        public void FulfilledCondition_ANDNOT_NotFulfilledCondition_ShouldBeNotFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.AndNot, new FulfilledTestCondition());
            condition.Validate().ShouldBeEquivalentTo(ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void FulfilledCondition_ORNOT_FulfilledCondition_ShouldBeNotFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.OrNot, new FulfilledTestCondition());
            condition.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }

        [TestMethod]
        public void FulfilledCondition_ORNOT_NotFulfilledCondition_ShouldBeNotFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.OrNot, new NotFulfilledTestCondition());
            condition.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }

        [TestMethod]
        public void NotFulfilledCondition_ORNOT_FulfilledCondition_ShouldBeNotFulfilled()
        {
            var condition = new NotFulfilledTestCondition().WithRelatedCondition(ConditionRelation.OrNot, new FulfilledTestCondition());
            condition.Validate().ShouldBeEquivalentTo(ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void ConditionWithMultipleRelatedConditionsMustBeFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.And, new FulfilledTestCondition()).WithRelatedCondition(ConditionRelation.Or, new NotFulfilledTestCondition());
            condition.Validate().ShouldBeEquivalentTo(ConditionState.Fulfilled);
        }

        [TestMethod]
        public void ConditionWithMultipleRelatedConditionsMustBeNotFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.And, new FulfilledTestCondition()).WithRelatedCondition(ConditionRelation.And, new NotFulfilledTestCondition());
            condition.Validate().ShouldBeEquivalentTo(ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void ComponentHasStateCondition()
        {
            var stateMachineFactory = new TestStateMachineFactory();
            var output = stateMachineFactory.CreateTestStateMachineWithOnOffStates();

            var condition = new ComponentIsInStateCondition(output, BinaryStateId.Off);
            condition.IsFulfilled().ShouldBeEquivalentTo(true);

            output.SetNextState();

            condition.IsFulfilled().ShouldBeEquivalentTo(false);
        }
    }
}
