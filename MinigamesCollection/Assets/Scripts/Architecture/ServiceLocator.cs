using System;
using System.Collections.Generic;

namespace Architecture
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new();

        public static void Register<T>(T service) where T : class, IService
        {
            var type = typeof(T);
            if (!services.ContainsKey(type))
            {
                services[type] = service;
                service.Initialize();
            }
        }

        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (services.ContainsKey(type))
            {
                return services[type] as T;
            }
            return null;
        }
    }
}