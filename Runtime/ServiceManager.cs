using System;
using System.Collections.Generic;
using UnityEngine;

namespace Services.Runtime
{
    [Serializable]
    public class ServiceManager
    {
        private Dictionary<Type, IService> _services = new Dictionary<Type, IService>();

        /// <summary>
        /// Registers the service. This should be done after the service is initialized 
        /// </summary>
        public bool RegisterService<T>(IService service) where T : IService
        {
            if (_services.ContainsKey(typeof(T)))
            {
                Debug.LogWarning($"A service of type {typeof(T).Name} is already registered");
                return false;
            }
            
            _services.Add(typeof(T), service);
            return true;
        }

        /// <summary>
        /// Tries to get the registered service of the given type. Will return the service if it is found
        /// </summary>
        public bool TryGetService<T>(out IService service) where T : IService
        {
            if (_services.ContainsKey(typeof(T)))
            {
                service = _services[typeof(T)];
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Cleans up and remove the service if it is registered
        /// </summary>
        public bool RemoveService<T>() where T : IService
        {
            if (!_services.ContainsKey(typeof(T)))
            {
                return false;
            }
            
            _services[typeof(T)].Cleanup();
            _services.Remove(typeof(T));
            return true;
        }
    }
}
