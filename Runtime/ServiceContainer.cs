using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Services.Runtime
{
    public class ServiceContainer
    {
        private readonly Dictionary<Type, IService> Services = new Dictionary<Type, IService>();
        
        #if UNITY_EDITOR
        /// <summary>
        /// An editor-only hook for getting all active <see cref="IService"/>s that are currently registered
        /// </summary>
        public Dictionary<Type, IService> EditorActiveServices => Services;

        /// <summary>
        /// An editor-only hook to view all active <see cref="ServiceContainer"/>s
        /// </summary>
        public static List<ServiceContainer> EditorActiveContainers { get; } = new();
        #endif

        private IService _serviceCache;

        public ServiceContainer()
        {
            #if UNITY_EDITOR
            EditorActiveContainers.Add(this);
            #endif
            
            Application.quitting += StopAllServices;
        }

        /// <summary>
        /// Starts the <see cref="IService"/> of the given type if it is not already running and returns the active instance
        /// </summary>
        public async Task<T> GetService<T>() where T : IService
        {
            if (!HasService<T>())
            {
                var newService = (T)((typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, Type.EmptyTypes, null)?.Invoke(null)));
                if (newService == null)
                {
                    LogError($"Failed to start service\nService type: {typeof(T).FullName}");
                    return default;
                }

                Log($"Service starting: {typeof(T).Name}");

                newService.Terminated += OnServiceTerminated;
                Services.Add(typeof(T), newService);
            }

            _serviceCache = Services[typeof(T)];
            while (_serviceCache.State != ServiceState.Running)
            {
                await Task.Yield();
            }
            
            Log($"Service running: {typeof(T).Name}");

            return (T)_serviceCache;
        }

        /// <summary>
        /// A light-weight way of checking if a service is active
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool HasService<T>() where T : IService
        {
            return Services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Tries to get the registered service of the given type. Will return the service if it is found
        /// </summary>
        public bool TryGetService<T>(out T service) where T : IService
        {
            if (Services.ContainsKey(typeof(T)))
            {
                service = (T)Services[typeof(T)];
                return true;
            }

            service = default;
            return false;
        }
        
        /// <summary>
        /// Stops all services currently registered
        /// </summary>
        public void StopAllServices()
        {
            var services = Services.Keys.ToArray();
            foreach (var service in services)
            {
                Services[service].Shutdown();
            }
        }

        /// <summary>
        /// Cleans up and remove the service if it is registered
        /// </summary>
        public async Task StopService<T>() where T : IService
        {
            if (TryGetService<T>(out var service))
            {
                if (service.State != ServiceState.Stopping && service.State != ServiceState.Stopped)
                {
                    Log($"Stopping service: {typeof(T).Name}");
                    service.Shutdown();
                }
                
                while (service.State != ServiceState.Stopped)
                {
                    await Task.Yield();
                }
                
                Log($"Service stopped: {typeof(T).Name}");

                return;
            }

            LogWarning($"Trying to shutdown a service that isn't registered\nService Type: {typeof(T).FullName}");
        }

        /// <summary>
        /// When an <see cref="IService"/> is terminated, listen to it's delegate and remove the service for the list
        /// of registered <see cref="IService"/>s
        /// </summary>
        /// <param name="sender">The instance of the <see cref="IService"/></param>
        /// <param name="eventArgs">This should always be <see cref="EventArgs.Empty"/></param>
        private void OnServiceTerminated(object sender, EventArgs eventArgs)
        {
            var service = (IService)sender;
            Services.Remove(service.GetType());
        }

        #region LOGGING
        [Conditional("SERVICES_LOG")]
        private static void Log(string message)
        {
            Debug.Log($"[SERVICES] {message}");
        }

        [Conditional("SERVICES_LOGWARNING"), Conditional("DEBUG")]
        private static void LogWarning(string message)
        {
            Debug.LogWarning($"[SERVICES] {message}");
        }

        [Conditional("SERVICES_LOGERROR"), Conditional("DEBUG")]
        private static void LogError(string message)
        {
            Debug.LogError($"[SERVICES] {message}");
        }
        #endregion
    }
}
