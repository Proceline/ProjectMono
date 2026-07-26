using System;
using UnityEngine;
using UnityEngine.Events;

namespace MonopolyPrototype
{
    [Serializable]
    public sealed class VoidSOEventUnityEvent : UnityEvent
    {
    }

    [CreateAssetMenu(menuName = "Monopoly Prototype/SO Events/Void Event")]
    public sealed class VoidSOEvent : SOEvent
    {
        [SerializeField] private VoidSOEventUnityEvent onRaised = new VoidSOEventUnityEvent();

        private readonly OrderedEventListeners<UnityAction> runtimeListeners =
            new OrderedEventListeners<UnityAction>();

        public override int RuntimeListenerCount => runtimeListeners.Count;

        public void Register(UnityAction listener, int order = DefaultOrder)
        {
            runtimeListeners.Register(listener, order);
        }

        public bool Unregister(UnityAction listener)
        {
            return runtimeListeners.Unregister(listener);
        }

        public void Raise()
        {
            onRaised.Invoke();

            foreach (var listener in runtimeListeners.Snapshot())
            {
                listener.Invoke();
            }
        }

        public override void ClearRuntimeListeners()
        {
            runtimeListeners.Clear();
        }
    }
}
