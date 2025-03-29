#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReactiveUnity
{
    [Serializable]
    public class ReactiveStack<T> : IEnumerable<T>
    {
        private List<Action<T>> _pushCbs = new();
        private List<Action<T>> _popCbs = new();
        private List<Action<T>> _changeCbs = new();

        [SerializeField]
        private List<T> _internal = new(); // list backed to be supported by Unitys serialization system

        public int Count => _internal.Count;

        public Action OnPush(Action<T> cb)
        {
            _pushCbs.Add(cb);
            return () => _pushCbs.Remove(cb);
        }

        public Action OnPop(Action<T> cb)
        {
            _popCbs.Add(cb);
            return () => _popCbs.Remove(cb);
        }

        public Action OnChange(Action<T> cb)
        {
            _changeCbs.Add(cb);
            return () => _changeCbs.Remove(cb);
        }

        public void Push(T item)
        {
            _internal.Add(item);

            foreach (var cb in _pushCbs)
            {
                cb(item);
            }

            foreach (var cb in _changeCbs)
            {
                cb(item);
            }
        }

        public T Pop()
        {
            if (_internal.Count == 0)
                throw new InvalidOperationException("Cannot pop from an empty stack.");

            T item = _internal[^1];
            _internal.RemoveAt(_internal.Count - 1);

            foreach (var cb in _popCbs)
            {
                cb(item);
            }

            foreach (var cb in _changeCbs)
            {
                cb(item);
            }

            return item;
        }

        public T Peek()
        {
            if (_internal.Count == 0)
                throw new InvalidOperationException("Cannot peek from an empty stack.");

            return _internal[^1];
        }

        public void Clear()
        {
            List<T> copy = new List<T>(_internal);
            _internal.Clear();

            foreach (var item in copy)
            {
                foreach (var cb in _popCbs)
                {
                    cb(item);
                }
            }

            foreach (var cb in _changeCbs)
            {
                cb(default!);
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            return _internal.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}