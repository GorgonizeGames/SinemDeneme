using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Game.Runtime.Core.DI
{
    public class DIContainer
    {
        // Thread-safe dictionary to prevent race conditions
        private readonly ConcurrentDictionary<Type, object> _services = new ConcurrentDictionary<Type, object>();

        // Cached reflection data to avoid repeated GetMethod calls
        private readonly ConcurrentDictionary<Type, System.Reflection.MethodInfo> _resolveMethodCache =
            new ConcurrentDictionary<Type, System.Reflection.MethodInfo>();

        public void Register<T>(T service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service), $"Cannot register null service for type {typeof(T)}");
            }

            _services.AddOrUpdate(typeof(T), service, (key, oldValue) => service);
        }

        public T Resolve<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                try
                {
                    return (T)service;
                }
                catch (InvalidCastException e)
                {
                    throw new InvalidOperationException($"Service registered for type {typeof(T)} cannot be cast to {typeof(T)}", e);
                }
            }

            throw new InvalidOperationException($"Service of type {typeof(T)} not registered.");
        }

        public bool TryResolve<T>(out T service)
        {
            service = default(T);

            if (_services.TryGetValue(typeof(T), out var obj))
            {
                try
                {
                    service = (T)obj;
                    return true;
                }
                catch (InvalidCastException)
                {
                    return false;
                }
            }

            return false;
        }

        public bool IsRegistered<T>()
        {
            return _services.ContainsKey(typeof(T));
        }

        public bool IsRegistered(Type type)
        {
            return _services.ContainsKey(type);
        }

        public void Unregister<T>()
        {
            _services.TryRemove(typeof(T), out _);
        }

        public void Clear()
        {
            _services.Clear();
            _resolveMethodCache.Clear();
        }

        public int ServiceCount => _services.Count;
    }
}