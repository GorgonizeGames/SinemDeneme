using System;
using System.Collections.Generic;

namespace Game.Runtime.Core.DI
{
    public class DIContainer
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void Register<T>(T service)
        {
            _services[typeof(T)] = service;
        }

        public T Resolve<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            throw new Exception($"[DIContainer] Service of type {typeof(T)} not registered.");
        }
    }
}