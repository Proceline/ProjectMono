using System;
using UnityEngine;
using UnityEngine.Events;

namespace MonopolyPrototype
{
    [Serializable]
    public sealed class IntSOEventUnityEvent : UnityEvent<int>
    {
    }

    [CreateAssetMenu(menuName = "Monopoly Prototype/SO Events/Int Event")]
    public sealed class IntSOEvent : SOEvent
    {
        [SerializeField] private IntSOEventUnityEvent onRaised = new IntSOEventUnityEvent();

        private readonly OrderedEventListeners<UnityAction<int>> runtimeListeners =
            new OrderedEventListeners<UnityAction<int>>();

        public override int RuntimeListenerCount => runtimeListeners.Count;

        public void Register(UnityAction<int> listener, int order = DefaultOrder)
        {
            runtimeListeners.Register(listener, order);
        }

        public bool Unregister(UnityAction<int> listener)
        {
            return runtimeListeners.Unregister(listener);
        }

        public void Raise(int value)
        {
            onRaised.Invoke(value);

            foreach (var listener in runtimeListeners.Snapshot())
            {
                listener.Invoke(value);
            }
        }

        public override void ClearRuntimeListeners()
        {
            runtimeListeners.Clear();
        }
    }
}
