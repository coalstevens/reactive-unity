#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReactiveUnity
{
    [Serializable]
    public class ReactiveCollection<T> : IReactiveCallbackOwner<T>
    {
        [SerializeField]
        protected T? _val;
        public T? Value
        {
            get { return _val; }
        }

        protected virtual bool EqualityFunc(T? first, T? second)
        {
            if (first is null && second is null)
            {
                return true;
            }

            if (first is null && second is not null)
            {
                return false;
            }

            if (first is not null && second is null)
            {
                return false;
            }

#pragma warning disable 8604
            return EqualityComparer<T>.Default.Equals(first, second);
#pragma warning restore 8604
        }

        protected virtual void FlushAdditionalCbs(T? prev, T? curr) { }

        private List<Action<T?, T?>> _cbs = new List<Action<T?, T?>>();

        protected void Set(T? to)
        {
            T? prevValue = _val;
            _val = to;

            if (EqualityFunc(prevValue, _val))
            {
                return;
            }

            FlushCallbacks(prevValue, _val);
        }

        private void FlushCallbacks(T? prevVal, T? val)
        {
            int numCbs = _cbs.Count;
            for (int i = 0; i < numCbs; i++)
            {
                _cbs[i](prevVal, val);
            }
            FlushAdditionalCbs(prevVal, val);
        }

        private void ForceFlushCallbacks()
        {
            FlushCallbacks(default(T), _val);
        }

        public Action OnChange(Action<T?, T?> cb)
        {
            _cbs.Add(cb);
            return () => _cbs.Remove(cb);
        }

        public Action OnChange(Action<T?> cb)
        {
            Action<T?, T?> curried = (prev, curr) => cb(curr);
            _cbs.Add(curried);
            return () => _cbs.Remove(curried);
        }
    }
}
#nullable disable
