using System;
using UnityEngine;
using UnityEngine.Events;

namespace MonopolyPrototype
{
    [Serializable]
    public sealed class MoneyChangeRequestedUnityEvent : UnityEvent<MoneyChangeRequest>
    {
    }

    [CreateAssetMenu(menuName = "Monopoly Prototype/SO Events/Money Change Requested")]
    public sealed class MoneyChangeRequestedSOEvent : SOEvent
    {
        [SerializeField] private MoneyChangeRequestedUnityEvent onRaised =
            new MoneyChangeRequestedUnityEvent();

        private readonly OrderedEventListeners<UnityAction<MoneyChangeRequest>> runtimeListeners =
            new OrderedEventListeners<UnityAction<MoneyChangeRequest>>();

        public override int RuntimeListenerCount => runtimeListeners.Count;

        public void Register(UnityAction<MoneyChangeRequest> listener, int order = DefaultOrder)
        {
            runtimeListeners.Register(listener, order);
        }

        public bool Unregister(UnityAction<MoneyChangeRequest> listener)
        {
            return runtimeListeners.Unregister(listener);
        }

        public void Raise(MoneyChangeRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            onRaised.Invoke(request);

            foreach (var listener in runtimeListeners.Snapshot())
            {
                listener.Invoke(request);
            }
        }

        public override void ClearRuntimeListeners()
        {
            runtimeListeners.Clear();
        }
    }
}
