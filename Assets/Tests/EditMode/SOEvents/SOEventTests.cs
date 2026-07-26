using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

namespace MonopolyPrototype
{
    public sealed class SOEventTests
    {
        [Test]
        public void VoidEvent_RaisesRuntimeListenersByOrderAndStableRegistration()
        {
            var soEvent = ScriptableObject.CreateInstance<VoidSOEvent>();
            try
            {
                var calls = new List<string>();
                UnityAction first = () => calls.Add("first");
                UnityAction second = () => calls.Add("second");
                UnityAction third = () => calls.Add("third");

                soEvent.Register(first, order: 10);
                soEvent.Register(second, order: -5);
                soEvent.Register(third, order: 10);

                soEvent.Raise();

                CollectionAssert.AreEqual(new[] { "second", "first", "third" }, calls);
                Assert.That(soEvent.RuntimeListenerCount, Is.EqualTo(3));
                Assert.That(soEvent.Unregister(first), Is.True);
                Assert.That(soEvent.Unregister(first), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(soEvent);
            }
        }

        [Test]
        public void IntEvent_UnregisterStopsFutureRaises()
        {
            var soEvent = ScriptableObject.CreateInstance<IntSOEvent>();
            try
            {
                var values = new List<int>();
                UnityAction<int> listener = value => values.Add(value);

                soEvent.Register(listener);
                soEvent.Raise(7);
                Assert.That(soEvent.Unregister(listener), Is.True);
                soEvent.Raise(9);

                CollectionAssert.AreEqual(new[] { 7 }, values);
            }
            finally
            {
                Object.DestroyImmediate(soEvent);
            }
        }

        [Test]
        public void IntArrayEvent_PassesTheSameMutableArrayToListeners()
        {
            var soEvent = ScriptableObject.CreateInstance<IntArraySOEvent>();
            try
            {
                int[] received = null;
                var receivedFirstValue = 0;
                UnityAction<int[]> mutate = values => values[0] = 42;
                UnityAction<int[]> observe = values =>
                {
                    received = values;
                    receivedFirstValue = values[0];
                };

                soEvent.Register(mutate, order: 0);
                soEvent.Register(observe, order: 0);

                var payload = new[] { 1, 2 };
                soEvent.Raise(payload);

                Assert.That(received, Is.SameAs(payload));
                Assert.That(receivedFirstValue, Is.EqualTo(42));
                Assert.That(payload[0], Is.EqualTo(42));
            }
            finally
            {
                Object.DestroyImmediate(soEvent);
            }
        }

        [Test]
        public void IntArrayIntEvent_SupportsArrayAndAdditionalParameters()
        {
            var soEvent = ScriptableObject.CreateInstance<IntArrayIntSOEvent>();
            try
            {
                int[] receivedValues = null;
                var receivedContext = 0;
                UnityAction<int[], int> listener = (values, context) =>
                {
                    receivedValues = values;
                    receivedContext = context;
                };

                soEvent.Register(listener);
                var payload = new[] { 3, 4 };
                soEvent.Raise(payload, 12);

                Assert.That(receivedValues, Is.SameAs(payload));
                Assert.That(receivedContext, Is.EqualTo(12));
            }
            finally
            {
                Object.DestroyImmediate(soEvent);
            }
        }

        [Test]
        public void ClearRuntimeListeners_RemovesAllRegisteredCallbacks()
        {
            var soEvent = ScriptableObject.CreateInstance<VoidSOEvent>();
            try
            {
                soEvent.Register(() => { });
                soEvent.Register(() => { });

                soEvent.ClearRuntimeListeners();

                Assert.That(soEvent.RuntimeListenerCount, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(soEvent);
            }
        }
    }
}
