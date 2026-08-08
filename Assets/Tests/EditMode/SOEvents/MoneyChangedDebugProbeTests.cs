using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;

namespace MonopolyPrototype
{
    public sealed class MoneyChangedDebugProbeTests
    {
        [Test]
        public void RequestProbe_AddMoneyMutatesSharedPayloadForLaterListener()
        {
            var requestEvent = ScriptableObject.CreateInstance<MoneyChangeRequestedSOEvent>();
            var probe = ScriptableObject.CreateInstance<MoneyChangedDebugProbeSO>();
            try
            {
                probe.Configure(25, 10, 0);
                int[] observedPayload = null;
                UnityAction<MoneyChangeRequest> addMoney = probe.AddMoney;
                UnityAction<MoneyChangeRequest> observe = request => observedPayload = request.CurrentDeltaPayload;
                requestEvent.Register(addMoney, order: 0);
                requestEvent.Register(observe, order: 10);

                var request = CreateRequest(100);
                var originalPayload = request.CurrentDeltaPayload;

                requestEvent.Raise(request);

                Assert.That(observedPayload, Is.SameAs(originalPayload));
                Assert.That(observedPayload[0], Is.EqualTo(125));
                Assert.That(request.CurrentDelta, Is.EqualTo(125));
            }
            finally
            {
                Object.DestroyImmediate(probe);
                Object.DestroyImmediate(requestEvent);
            }
        }

        [Test]
        public void ArrayProbe_SubtractMoneyMutatesTheSamePayloadForLaterListener()
        {
            var soEvent = ScriptableObject.CreateInstance<IntArraySOEvent>();
            var probe = ScriptableObject.CreateInstance<MoneyChangedDebugProbeSO>();
            try
            {
                probe.Configure(25, 10, 0);
                int[] observedPayload = null;
                UnityAction<int[]> subtractMoney = probe.SubtractMoney;
                UnityAction<int[]> observe = values => observedPayload = values;
                soEvent.Register(subtractMoney, order: 0);
                soEvent.Register(observe, order: 10);

                var payload = new[] { 100 };
                soEvent.Raise(payload);

                Assert.That(observedPayload, Is.SameAs(payload));
                Assert.That(payload[0], Is.EqualTo(90));
            }
            finally
            {
                Object.DestroyImmediate(probe);
                Object.DestroyImmediate(soEvent);
            }
        }

        [Test]
        public void ArrayProbe_OverrideMoneyReplacesThePayloadAmount()
        {
            var probe = ScriptableObject.CreateInstance<MoneyChangedDebugProbeSO>();
            try
            {
                probe.Configure(25, 10, 7);
                var payload = new[] { 100 };

                probe.OverrideMoney(payload);

                Assert.That(payload[0], Is.EqualTo(7));
            }
            finally
            {
                Object.DestroyImmediate(probe);
            }
        }

        [Test]
        public void RequestProbe_SubtractAndOverrideMutateRequestPayload()
        {
            var probe = ScriptableObject.CreateInstance<MoneyChangedDebugProbeSO>();
            try
            {
                probe.Configure(25, 10, 7);
                var request = CreateRequest(100);

                probe.SubtractMoney(request);
                Assert.That(request.CurrentDelta, Is.EqualTo(90));

                probe.OverrideMoney(request);
                Assert.That(request.CurrentDelta, Is.EqualTo(7));
            }
            finally
            {
                Object.DestroyImmediate(probe);
            }
        }

        [Test]
        public void RequestPayload_SetCurrentDeltaKeepsTheSameArrayReference()
        {
            var request = CreateRequest(100);
            var payload = request.CurrentDeltaPayload;

            payload[0] = 125;
            request.SetCurrentDelta(80);

            Assert.That(request.CurrentDeltaPayload, Is.SameAs(payload));
            Assert.That(payload[0], Is.EqualTo(80));
            Assert.That(request.CurrentDelta, Is.EqualTo(80));
        }

        [Test]
        public void ArrayProbe_LogMoneyWritesTheCurrentAmountToTheUnityLog()
        {
            var probe = ScriptableObject.CreateInstance<MoneyChangedDebugProbeSO>();
            try
            {
                LogAssert.Expect(LogType.Log, "Money debug amount: 123.");

                probe.LogMoney(new[] { 123 });
            }
            finally
            {
                Object.DestroyImmediate(probe);
            }
        }

        [Test]
        public void RequestProbe_LogMoneyWritesTheRequestAmountToTheUnityLog()
        {
            var probe = ScriptableObject.CreateInstance<MoneyChangedDebugProbeSO>();
            try
            {
                LogAssert.Expect(LogType.Log, "Money debug amount: 123.");

                probe.LogMoney(CreateRequest(123));
            }
            finally
            {
                Object.DestroyImmediate(probe);
            }
        }

        private static MoneyChangeRequest CreateRequest(int delta)
        {
            return new MoneyChangeRequest(
                "Bank",
                "Bank_AdjustMoney",
                BuildingEffectType.AdjustMoney,
                1,
                4,
                MoveEventTiming.Stop,
                0,
                delta);
        }
    }
}
