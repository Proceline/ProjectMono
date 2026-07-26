using System;
using System.Collections.Generic;

namespace MonopolyPrototype
{
    internal sealed class OrderedEventListeners<TDelegate> where TDelegate : class
    {
        private readonly List<ListenerEntry> listeners = new List<ListenerEntry>();
        private long nextRegistrationSequence;

        public int Count => listeners.Count;

        public void Register(TDelegate listener, int order)
        {
            if (ReferenceEquals(listener, null))
            {
                throw new ArgumentNullException(nameof(listener));
            }

            for (var index = 0; index < listeners.Count; index++)
            {
                if (Equals(listeners[index].Listener, listener))
                {
                    listeners[index].Order = order;
                    return;
                }
            }

            listeners.Add(new ListenerEntry(listener, order, nextRegistrationSequence++));
        }

        public bool Unregister(TDelegate listener)
        {
            if (ReferenceEquals(listener, null))
            {
                return false;
            }

            for (var index = 0; index < listeners.Count; index++)
            {
                if (Equals(listeners[index].Listener, listener))
                {
                    listeners.RemoveAt(index);
                    return true;
                }
            }

            return false;
        }

        public TDelegate[] Snapshot()
        {
            var orderedEntries = new List<ListenerEntry>(listeners);
            orderedEntries.Sort(CompareEntries);

            var snapshot = new TDelegate[orderedEntries.Count];
            for (var index = 0; index < orderedEntries.Count; index++)
            {
                snapshot[index] = orderedEntries[index].Listener;
            }

            return snapshot;
        }

        public void Clear()
        {
            listeners.Clear();
        }

        private static int CompareEntries(ListenerEntry left, ListenerEntry right)
        {
            var orderComparison = left.Order.CompareTo(right.Order);
            return orderComparison != 0
                ? orderComparison
                : left.RegistrationSequence.CompareTo(right.RegistrationSequence);
        }

        private sealed class ListenerEntry
        {
            public ListenerEntry(TDelegate listener, int order, long registrationSequence)
            {
                Listener = listener;
                Order = order;
                RegistrationSequence = registrationSequence;
            }

            public TDelegate Listener { get; }
            public int Order { get; set; }
            public long RegistrationSequence { get; }
        }
    }
}
