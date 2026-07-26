using System;
using UnityEngine;
using UnityEngine.Events;

namespace MonopolyPrototype
{
    [Serializable]
    public sealed class IntArrayIntSOEventUnityEvent : UnityEvent<int[], int>
    {
    }

    [CreateAssetMenu(menuName = "Monopoly Prototype/SO Events/Int Array + Int Event")]
    public sealed class IntArrayIntSOEvent : SOEvent
    {
        [SerializeField] private IntArrayIntSOEventUnityEvent onRaised =
            new IntArrayIntSOEventUnityEvent();

        private readonly OrderedEventListeners<UnityAction<int[], int>> runtimeListeners =
            new OrderedEventListeners<UnityAction<int[], int>>();

        public override int RuntimeListenerCount => runtimeListeners.Count;

        public void Register(UnityAction<int[], int> listener, int order = DefaultOrder)
        {
            runtimeListeners.Register(listener, order);
        }

        public bool Unregister(UnityAction<int[], int> listener)
        {
            return runtimeListeners.Unregister(listener);
        }

        public void Raise(int[] values, int context)
        {
            onRaised.Invoke(values, context);

            foreach (var listener in runtimeListeners.Snapshot())
            {
                listener.Invoke(values, context);
            }
        }

        public override void ClearRuntimeListeners()
        {
            runtimeListeners.Clear();
        }
    }
}
