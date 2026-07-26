using System;
using UnityEngine;
using UnityEngine.Events;

namespace MonopolyPrototype
{
    [Serializable]
    public sealed class IntArraySOEventUnityEvent : UnityEvent<int[]>
    {
    }

    [CreateAssetMenu(menuName = "Monopoly Prototype/SO Events/Int Array Event")]
    public sealed class IntArraySOEvent : SOEvent
    {
        [SerializeField] private IntArraySOEventUnityEvent onRaised =
            new IntArraySOEventUnityEvent();

        private readonly OrderedEventListeners<UnityAction<int[]>> runtimeListeners =
            new OrderedEventListeners<UnityAction<int[]>>();

        public override int RuntimeListenerCount => runtimeListeners.Count;

        public void Register(UnityAction<int[]> listener, int order = DefaultOrder)
        {
            runtimeListeners.Register(listener, order);
        }

        public bool Unregister(UnityAction<int[]> listener)
        {
            return runtimeListeners.Unregister(listener);
        }

        public void Raise(int[] values)
        {
            onRaised.Invoke(values);

            foreach (var listener in runtimeListeners.Snapshot())
            {
                listener.Invoke(values);
            }
        }

        public override void ClearRuntimeListeners()
        {
            runtimeListeners.Clear();
        }
    }
}
