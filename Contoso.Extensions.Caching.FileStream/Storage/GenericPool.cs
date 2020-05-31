using System;
using System.Collections.Concurrent;

namespace Contoso.Extensions.Caching.FileStream
{
    public delegate void GenericPoolAction<T>(Action<T> action);
    public delegate TResult GenericPoolFunc<T, TResult>(Func<T, TResult> action);

    public class GenericPool<T> : IDisposable
        where T : IDisposable
    {
        const int MAX = 10;
        readonly ConcurrentBag<T> _items = new ConcurrentBag<T>();
        public readonly Func<T> Factory;

        public GenericPool(Func<T> factory) => Factory = factory;

        public void Dispose()
        {
            foreach (var item in _items)
                item.Dispose();
        }

        public void Release(T item)
        {
            if (_items.Count < MAX) _items.Add(item);
            else item.Dispose();
        }

        public T Get() => _items.TryTake(out var item) ? item : Factory();

        public void Action(Action<T> action)
        {
            var item = Get();
            try { action(item); }
            finally { Release(item); }
        }

        public TResult Func<TResult>(Func<T, TResult> action)
        {
            var item = Get();
            try { return action(item); }
            finally { Release(item); }
        }
    }
}