using System;
using System.Collections.Generic;
using System.Reflection;
using Events.Runtime;
using UnityEngine;

namespace Services.Runtime
{
    public static class ServiceManager
    {
        private static readonly Dictionary<Type, IService> Services = new Dictionary<Type, IService>();
        
        #if UNITY_EDITOR
        /// <summary>
        /// An editor-only hook for getting all active <see cref="IService"/>s that are currently registered
        /// </summary>
        public static Dictionary<Type, IService> EditorActiveServices => Services;
        #endif

        /// <summary>
        /// Starts the <see cref="IService"/> of the given type if it is not already running and returns the active instance
        /// </summary>
        public static T StartService<T>() where T : IService
        {
            if (!HasService<T>())
            {
                Debug.LogWarning($"A service of type {typeof(T).FullName} is already registered");
                var newService = (T)((typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, Type.EmptyTypes, null)?.Invoke(null)));
                if (newService == null)
                {
                    Debug.LogError($"Failed to start service\nService type: {typeof(T).FullName}");
                    return default;
                }
                else
                {
                    Debug.Log($"Service started: {typeof(T).Name}");
                }
                
                newService.Terminated += OnServiceTerminated;
                Services.Add(typeof(T), newService);
                EventManager.TriggerEvent(new ServiceRegisteredEvent((T)Services[typeof(T)]));
            }

            return (T)Services[typeof(T)];
        }

        /// <summary>
        /// A light-weight way of checking if a service is active
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool HasService<T>() where T : IService
        {
            return Services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Tries to get the registered service of the given type. Will return the service if it is found
        /// </summary>
        public static bool TryGetService<T>(out T service) where T : IService
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
        /// Cleans up and remove the service if it is registered
        /// </summary>
        public static bool StopService<T>() where T : IService
        {
            if (TryGetService<T>(out var service))
            {
                EventManager.TriggerEvent(new ServiceStoppedEvent(service));
                service.Shutdown();
                return true;
            }

            Debug.LogWarning($"Trying to shutdown a service that isn't registered\nService Type: {typeof(T).FullName}");
            return false;
        }

        /// <summary>
        /// When an <see cref="IService"/> is terminated, listen to it's delegate and remove the service for the list
        /// of registered <see cref="IService"/>s
        /// </summary>
        /// <param name="sender">The instance of the <see cref="IService"/></param>
        /// <param name="eventArgs">This should always be <see cref="EventArgs.Empty"/></param>
        private static void OnServiceTerminated(object sender, EventArgs eventArgs)
        {
            var service = (IService)sender;
            Services.Remove(service.GetType());
        }
    }
}
