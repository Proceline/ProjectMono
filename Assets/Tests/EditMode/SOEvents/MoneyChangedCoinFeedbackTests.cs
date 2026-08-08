using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

namespace MonopolyPrototype
{
    public sealed class MoneyChangedCoinFeedbackTests
    {
        [Test]
        public void ConfigureBindsToMoneyChangedEventAndFormatsPositiveAppliedDelta()
        {
            var moneyChangedEvent = ScriptableObject.CreateInstance<MoneyChangedSOEvent>();
            var feedbackObject = new GameObject("Money Changed Coin Feedback Test");
            var feedback = feedbackObject.AddComponent<MoneyChangedCoinFeedback>();
            try
            {
                feedback.Configure(moneyChangedEvent);
                var result = CreateResult(100, 25, true);

                moneyChangedEvent.Raise(result);

                Assert.That(moneyChangedEvent.RuntimeListenerCount, Is.EqualTo(1));
                Assert.That(feedback.FeedbackCount, Is.EqualTo(1));
                Assert.That(feedback.LastResult, Is.SameAs(result));
                Assert.That(feedback.LastFeedbackText, Is.EqualTo("+25"));
            }
            finally
            {
                Object.DestroyImmediate(feedbackObject);
                Object.DestroyImmediate(moneyChangedEvent);
            }
        }

        [Test]
        public void NegativeAppliedDeltaFormatsAsNegativeFeedback()
        {
            var moneyChangedEvent = ScriptableObject.CreateInstance<MoneyChangedSOEvent>();
            var feedbackObject = new GameObject("Money Changed Coin Feedback Test");
            var feedback = feedbackObject.AddComponent<MoneyChangedCoinFeedback>();
            try
            {
                feedback.Configure(moneyChangedEvent);
                moneyChangedEvent.Raise(CreateResult(-40, -30, true));

                Assert.That(feedback.LastFeedbackText, Is.EqualTo("-30"));
            }
            finally
            {
                Object.DestroyImmediate(feedbackObject);
                Object.DestroyImmediate(moneyChangedEvent);
            }
        }

        [Test]
        public void DisableUnregistersFromMoneyChangedEvent()
        {
            var moneyChangedEvent = ScriptableObject.CreateInstance<MoneyChangedSOEvent>();
            var feedbackObject = new GameObject("Money Changed Coin Feedback Test");
            var feedback = feedbackObject.AddComponent<MoneyChangedCoinFeedback>();
            try
            {
                feedback.Configure(moneyChangedEvent);
                feedback.enabled = false;

                moneyChangedEvent.Raise(CreateResult(100, 25, true));

                Assert.That(moneyChangedEvent.RuntimeListenerCount, Is.Zero);
                Assert.That(feedback.FeedbackCount, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(feedbackObject);
                Object.DestroyImmediate(moneyChangedEvent);
            }
        }

        [Test]
        public void ConfigureSwitchesMoneyChangedEventWithoutDuplicateListeners()
        {
            var firstEvent = ScriptableObject.CreateInstance<MoneyChangedSOEvent>();
            var secondEvent = ScriptableObject.CreateInstance<MoneyChangedSOEvent>();
            var feedbackObject = new GameObject("Money Changed Coin Feedback Test");
            var feedback = feedbackObject.AddComponent<MoneyChangedCoinFeedback>();
            try
            {
                feedback.Configure(firstEvent);
                feedback.Configure(secondEvent);

                firstEvent.Raise(CreateResult(100, 25, true));
                secondEvent.Raise(CreateResult(100, 50, true));

                Assert.That(firstEvent.RuntimeListenerCount, Is.Zero);
                Assert.That(secondEvent.RuntimeListenerCount, Is.EqualTo(1));
                Assert.That(feedback.FeedbackCount, Is.EqualTo(1));
                Assert.That(feedback.LastFeedbackText, Is.EqualTo("+50"));
            }
            finally
            {
                Object.DestroyImmediate(feedbackObject);
                Object.DestroyImmediate(firstEvent);
                Object.DestroyImmediate(secondEvent);
            }
        }

        [Test]
        public void FeedbackDoesNotSubscribeToMoneyChangeRequestEvent()
        {
            var moneyChangedEvent = ScriptableObject.CreateInstance<MoneyChangedSOEvent>();
            var requestEvent = ScriptableObject.CreateInstance<MoneyChangeRequestedSOEvent>();
            var feedbackObject = new GameObject("Money Changed Coin Feedback Test");
            var feedback = feedbackObject.AddComponent<MoneyChangedCoinFeedback>();
            try
            {
                feedback.Configure(moneyChangedEvent);
                UnityAction<MoneyChangeRequest> requestObserver = _ => { };
                requestEvent.Register(requestObserver);

                requestEvent.Raise(CreateRequest(100));

                Assert.That(feedback.FeedbackCount, Is.Zero);
                Assert.That(requestEvent.RuntimeListenerCount, Is.EqualTo(1));
                Assert.That(moneyChangedEvent.RuntimeListenerCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(feedbackObject);
                Object.DestroyImmediate(moneyChangedEvent);
                Object.DestroyImmediate(requestEvent);
            }
        }

        private static MoneyChangeResult CreateResult(int requestDelta, int appliedDelta, bool succeeded)
        {
            return new MoneyChangeResult(
                CreateRequest(requestDelta),
                appliedDelta,
                balanceBefore: 100,
                balanceAfter: 100 + appliedDelta,
                succeeded: succeeded);
        }

        private static MoneyChangeRequest CreateRequest(int baseDelta)
        {
            return new MoneyChangeRequest(
                "Bank",
                "Bank_AdjustMoney",
                BuildingEffectType.AdjustMoney,
                1,
                4,
                MoveEventTiming.Stop,
                0,
                baseDelta);
        }
    }
}
