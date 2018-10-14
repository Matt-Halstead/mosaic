// TODO: dont really need this yet.

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Mosaic.Core
//{
//    // Rolling my own basic IoC container
//    public static class IoC
//    {
//        private static Dictionary<Type, object> _registeredInstances = new Dictionary<Type, object>();

//        public static void RegisterInstance<T>(object concreteInstance)
//            where T : class
//        {
//            RegisterInstance(typeof(T), concreteInstance);
//        }

//        public static void RegisterInstance(Type interfaceType, object concreteInstance)
//        {
//            if (interfaceType == null)
//            {
//                throw new ArgumentException($"{nameof(interfaceType)} must not be null.");
//            }

//            if (!interfaceType.IsInterface)
//            {
//                throw new ArgumentException($"{nameof(interfaceType)} must be an interface.");
//            }

//            if (concreteInstance == null)
//            {
//                throw new ArgumentException($"{nameof(concreteInstance)} must not be null.");
//            }

//            var concreteType = concreteInstance.GetType();
//            if (!interfaceType.IsAssignableFrom(concreteType))
//            {
//                throw new ArgumentException($"{concreteType.Name} is not an instance of interface {nameof(interfaceType)}.");
//            }

//            if (_registeredInstances.TryGetValue(interfaceType, out object value))
//            {
//                throw new ArgumentException($"{nameof(interfaceType)} is already mapped to instance of type {concreteType.Name}.");
//            }

//            _registeredInstances[interfaceType] = concreteInstance;
//        }

//        public static T Resolve<T>()
//            where T : class
//        {
//            if (!_registeredInstances.TryGetValue(typeof(T), out object value))
//            {
//                throw new ApplicationException($"No concrete instance registered for interface {typeof(T).Name}");
//            }

//            return value as T;
//        }
//    }
//}
