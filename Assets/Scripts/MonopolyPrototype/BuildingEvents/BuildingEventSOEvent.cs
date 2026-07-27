using System;
using UnityEngine;
using UnityEngine.Events;

namespace MonopolyPrototype
{
    [Serializable]
    public sealed class BuildingEventUnityEvent : UnityEvent<BuildingEventContext>
    {
    }

    [CreateAssetMenu(menuName = "Monopoly Prototype/SO Events/Building Event")]
    public sealed class BuildingEventSOEvent : SOEvent
    {
        [SerializeField] private BuildingEventUnityEvent onRaised = new BuildingEventUnityEvent();

        private readonly OrderedEventListeners<UnityAction<BuildingEventContext>> runtimeListeners =
            new OrderedEventListeners<UnityAction<BuildingEventContext>>();

        public override int RuntimeListenerCount => runtimeListeners.Count;

        public void Register(UnityAction<BuildingEventContext> listener, int order = DefaultOrder)
        {
            runtimeListeners.Register(listener, order);
        }

        public bool Unregister(UnityAction<BuildingEventContext> listener)
        {
            return runtimeListeners.Unregister(listener);
        }

        public void Raise(BuildingEventContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            onRaised.Invoke(context);

            foreach (var listener in runtimeListeners.Snapshot())
            {
                listener.Invoke(context);
            }
        }

        public override void ClearRuntimeListeners()
        {
            runtimeListeners.Clear();
        }
    }
}
