using System;
using System.Collections.Generic;

namespace Frontier.Core
{
    /// <summary>
    /// Unidirectional dependency enforcement registry.
    /// Prevents circular dependencies and provides service location.
    /// </summary>
    public class ServiceRegistry : IDisposable
    {
        private readonly Dictionary<Type, object> _services;
        private readonly HashSet<Type> _disposedServices;
        private readonly Stack<Type> _resolutionStack;
        private bool _isLocked;
        private bool _isDisposed;

        public bool IsLocked => _isLocked;
        public int ServiceCount => _services.Count;

        public ServiceRegistry()
        {
            _services = new Dictionary<Type, object>();
            _disposedServices = new HashSet<Type>();
            _resolutionStack = new Stack<Type>();
            _isLocked = false;
            _isDisposed = false;
        }

        /// <summary>
        /// Register a service instance. Can only be called before Lock().
        /// </summary>
        public void Register<T>(T service) where T : class
        {
            if (_isLocked)
                throw new InvalidOperationException("Cannot register services after registry is locked.");

            if (_isDisposed)
                throw new ObjectDisposedException(nameof(ServiceRegistry));

            var type = typeof(T);
            
            if (_services.ContainsKey(type))
            {
                var existing = _services[type];
                if (existing != null && !ReferenceEquals(existing, service))
                {
                    UnityEngine.Debug.LogWarning($"[ServiceRegistry] Replacing existing service of type {type.Name}");
                }
            }

            _services[type] = service;
        }

        /// <summary>
        /// Register a service with explicit interface type.
        /// </summary>
        public void Register<TInterface, TImplementation>(TImplementation service) 
            where TInterface : class 
            where TImplementation : class, TInterface
        {
            if (_isLocked)
                throw new InvalidOperationException("Cannot register services after registry is locked.");

            var interfaceType = typeof(TInterface);
            _services[interfaceType] = service;
            
            var implType = typeof(TImplementation);
            if (!_services.ContainsKey(implType))
            {
                _services[implType] = service;
            }
        }

        /// <summary>
        /// Resolve a service by type. Detects circular dependencies.
        /// </summary>
        public T Resolve<T>() where T : class
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(ServiceRegistry));

            var type = typeof(T);

            if (_resolutionStack.Contains(type))
            {
                string cycle = string.Join(" -> ", _resolutionStack) + " -> " + type.Name;
                throw new InvalidOperationException($"Circular dependency detected: {cycle}");
            }

            if (!_services.TryGetValue(type, out var service))
            {
                throw new KeyNotFoundException($"Service of type {type.Name} not registered.");
            }

            return (T)service;
        }

        /// <summary>
        /// Try to resolve a service, returning false if not found.
        /// </summary>
        public bool TryResolve<T>(out T service) where T : class
        {
            if (_isDisposed)
            {
                service = null;
                return false;
            }

            var type = typeof(T);

            if (_resolutionStack.Contains(type))
            {
                service = null;
                return false;
            }

            if (_services.TryGetValue(type, out var obj))
            {
                service = (T)obj;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Lock the registry to prevent further registrations.
        /// </summary>
        public void Lock()
        {
            _isLocked = true;
            UnityEngine.Debug.Log($"[ServiceRegistry] Locked with {_services.Count} services");
        }

        /// <summary>
        /// Check if a service type is registered.
        /// </summary>
        public bool IsRegistered<T>()
        {
            return _services.ContainsKey(typeof(T));
        }

        public bool IsRegistered(Type type)
        {
            return _services.ContainsKey(type);
        }

        /// <summary>
        /// Unregister a service (only allowed before lock).
        /// </summary>
        public void Unregister<T>()
        {
            if (_isLocked)
                throw new InvalidOperationException("Cannot unregister services after registry is locked.");

            _services.Remove(typeof(T));
        }

        public IEnumerable<Type> GetRegisteredTypes()
        {
            return _services.Keys;
        }

        public void Clear()
        {
            if (_isLocked)
                throw new InvalidOperationException("Cannot clear locked registry.");

            _services.Clear();
            _disposedServices.Clear();
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            foreach (var kvp in _services)
            {
                if (kvp.Value is IDisposable disposable && !_disposedServices.Contains(kvp.Key))
                {
                    try
                    {
                        disposable.Dispose();
                        _disposedServices.Add(kvp.Key);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"[ServiceRegistry] Error disposing {kvp.Key.Name}: {ex.Message}");
                    }
                }
            }

            _services.Clear();
            _disposedServices.Clear();
            _resolutionStack.Clear();
            _isDisposed = true;
        }

        ~ServiceRegistry()
        {
            Dispose();
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class AutoRegisterAttribute : Attribute
    {
        public Type[] Interfaces { get; }

        public AutoRegisterAttribute(params Type[] interfaces)
        {
            Interfaces = interfaces;
        }
    }
}
