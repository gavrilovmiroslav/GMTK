using System;
using System.Collections;
using System.Collections.Generic;

namespace GMTK
{
    public class Pool<T> : IEnumerable<T> where T : class, new()
    {
        public class PoolException : Exception
        {
            public PoolException(string message) : base($"[Pool<{typeof(T).Name}>]::{message}") { }
        }

        public delegate void PoolElementMutation(ref T element);

        private readonly T[] freelist;
        private readonly HashSet<T> acquired;

        public event PoolElementMutation OnAcquired;
        public event PoolElementMutation OnReleased;

        public int Capacity { get; private set; }
        public int Count { get; private set; }

        public Pool(int capacity)
        {
            if (capacity <= 0)
                throw new PoolException("[Constructor] Pool must have a capacity greater than zero.");

            this.Capacity = capacity;
            this.Count = 0;

            this.acquired = new HashSet<T>();
            this.freelist = new T[capacity];
            for (int i = 0; i < capacity; i++)
                this.freelist[i] = new T();
        }

        public T Acquire()
        {
            if (Count == Capacity)
                throw new PoolException("[Acquire] No more elements available in pool.");

            var element = this.freelist[Count++];
            OnAcquired?.Invoke(ref element);
            acquired.Add(element);

            return element;
        }

        public void Release(ref T element)
        {
            if (element == null)
                throw new PoolException("[Release] Element to release cannot be null.");

            if (Count == 0)
                throw new PoolException("[Release] Impossible to release element, pool is full.");

            if (!acquired.Remove(element))
                throw new PoolException("[Release] Element allocated outside of pool and cannot be released.");

            OnReleased?.Invoke(ref element);
            this.freelist[--Count] = element;

            element = null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)acquired).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)acquired).GetEnumerator();
        }
    }
}

