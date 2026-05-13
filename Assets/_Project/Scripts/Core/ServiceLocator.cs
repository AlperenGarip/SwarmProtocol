using System;
using System.Collections.Generic;

namespace SwarmProtocol.Core
{
    /// <summary>
    /// Lightweight service locator. All services are registered by Bootstrapper
    /// in Awake(). Consumers call ServiceLocator.Get&lt;T&gt;() at runtime.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(T service) where T : class
        {
            _services[typeof(T)] = service;
        }

        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;
            throw new Exception($"[ServiceLocator] Service '{typeof(T).Name}' is not registered.");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = null;
            return false;
        }

        /// <summary>Call on scene unload or game restart to avoid stale references.</summary>
        public static void Reset() => _services.Clear();
    }
}
