using UnityEngine;

namespace MonopolyPrototype
{
    public abstract class SOEvent : ScriptableObject
    {
        public const int DefaultOrder = 0;

        public abstract int RuntimeListenerCount { get; }

        public abstract void ClearRuntimeListeners();

        protected virtual void OnDisable()
        {
            ClearRuntimeListeners();
        }
    }
}
