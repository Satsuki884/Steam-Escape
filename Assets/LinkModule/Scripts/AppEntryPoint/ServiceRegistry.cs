using System;
using System.Collections.Generic;

namespace LinkModule.Scripts.AppEntryPoint
{
    /// <summary>
    ///     Defines a contract for a basic Dependency Injection (DI) service registry.
    ///     Allows registering, retrieving, and managing singleton services by their interface type.
    /// </summary>
    public interface IServiceRegistry
    {
        /// <summary>
        ///     Registers a service instance of type <typeparamref name="T" />.
        ///     If overwrite is false and the service already exists, an exception is thrown.
        /// </summary>
        /// <typeparam name="T">The interface or base type of the service.</typeparam>
        /// <param name="service">The instance to register.</param>
        /// <param name="overwrite">If true, replaces an existing service of the same type.</param>
        void Register<T>(T service, bool overwrite = false) where T : class;

        /// <summary>
        ///     Returns the registered instance of type <typeparamref name="T" />.
        ///     Throws if the service was not previously registered.
        /// </summary>
        /// <typeparam name="T">The service interface or base type.</typeparam>
        /// <returns>The service instance.</returns>
        T Get<T>() where T : class;

        /// <summary>
        ///     Attempts to get the service of type <typeparamref name="T" />.
        ///     Returns true and sets <paramref name="service" /> if found, otherwise false.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <param name="service">The output service reference, or null if not found.</param>
        /// <returns>True if service is registered.</returns>
        bool TryGet<T>(out T service) where T : class;

        /// <summary>
        ///     Checks if a service of type <typeparamref name="T" /> is currently registered.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <returns>True if registered, otherwise false.</returns>
        bool Contains<T>() where T : class;

        /// <summary>
        ///     Removes a service of type <typeparamref name="T" /> from the registry.
        ///     Does nothing if it is not registered.
        /// </summary>
        /// <typeparam name="T">The service type to unregister.</typeparam>
        void Unregister<T>() where T : class;

        /// <summary>
        ///     Removes all registered services from the registry.
        /// </summary>
        void Clear();

        /// <summary>
        ///     Retrieves a registered service by <see cref="Type" /> (non-generic).
        ///     Throws if not found.
        /// </summary>
        /// <param name="type">The service's type.</param>
        /// <returns>The service instance.</returns>
        object Get(Type type);

        /// <summary>
        ///     Gets an enumerable of all registered service types.
        /// </summary>
        IEnumerable<Type> RegisteredTypes { get; }
    }

    /// <summary>
    ///     Default implementation of <see cref="IServiceRegistry" /> for use in Unity projects.
    ///     Used for DI/service locator patterns. Not thread-safe.
    /// </summary>
    public class ServiceRegistry : IServiceRegistry
    {
        // Maps service interface/type to the singleton instance.
        private readonly Dictionary<Type, object> _services = new();

        /// <inheritdoc />
        public void Register<T>(T service, bool overwrite = false) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type) && !overwrite)
                throw new InvalidOperationException($"Service already registered: {type}");
            _services[type] = service;
        }

        /// <inheritdoc />
        public T Get<T>() where T : class
        {
            if (!_services.TryGetValue(typeof(T), out var svc))
                throw new InvalidOperationException($"Service of type {typeof(T).Name} not registered!");
            return (T)svc;
        }

        /// <inheritdoc />
        public bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var svc))
            {
                service = (T)svc;
                return true;
            }

            service = null;
            return false;
        }

        /// <inheritdoc />
        public bool Contains<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <inheritdoc />
        public void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
        }

        /// <inheritdoc />
        public void Clear()
        {
            _services.Clear();
        }

        /// <inheritdoc />
        public object Get(Type type)
        {
            if (!_services.TryGetValue(type, out var svc))
                throw new InvalidOperationException($"Service of type {type.Name} not registered!");
            return svc;
        }

        /// <inheritdoc />
        public IEnumerable<Type> RegisteredTypes => _services.Keys;
    }
}