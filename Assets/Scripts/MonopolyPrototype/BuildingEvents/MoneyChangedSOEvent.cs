using System;
using UnityEngine;
using UnityEngine.Events;

namespace MonopolyPrototype
{
    [Serializable]
    public sealed class MoneyChangedUnityEvent : UnityEvent<MoneyChangeResult>
    {
    }

    [CreateAssetMenu(menuName = "Monopoly Prototype/SO Events/Money Changed")]
    public sealed class MoneyChangedSOEvent : SOEvent
    {
        [SerializeField] private MoneyChangedUnityEvent onRaised = new MoneyChangedUnityEvent();

        private readonly OrderedEventListeners<UnityAction<MoneyChangeResult>> runtimeListeners =
            new OrderedEventListeners<UnityAction<MoneyChangeResult>>();

        public override int RuntimeListenerCount => runtimeListeners.Count;

        public void Register(UnityAction<MoneyChangeResult> listener, int order = DefaultOrder)
        {
            runtimeListeners.Register(listener, order);
        }

        public bool Unregister(UnityAction<MoneyChangeResult> listener)
        {
            return runtimeListeners.Unregister(listener);
        }

        public void Raise(MoneyChangeResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            onRaised.Invoke(result);

            foreach (var listener in runtimeListeners.Snapshot())
            {
                listener.Invoke(result);
            }
        }

        public override void ClearRuntimeListeners()
        {
            runtimeListeners.Clear();
        }
    }
}
