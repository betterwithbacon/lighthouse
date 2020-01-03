using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.ObjectModel;

namespace Lighthouse.Core.Utils
{
	public static class ReflectionUtil
	{
		public static IDictionary<Type, MethodInfo> GetMethodsBySingleParameterType(Type type, string methodName)
		{
			return type
				.GetMethods()
				.Where(mi =>
					mi.Name == methodName &&
					mi.GetParameters().Count() == 1)
				.ToDictionary(mi => mi.GetParameters().Single().ParameterType, mx => mx);				
		}
	}

    public class TypeFactory
    {
        private Dictionary<Type, Func<object>> TypeToFactories { get; } = new Dictionary<Type, Func<object>>();

        public void Register<T>(Func<object> factoryMethod)
        {
            TypeToFactories.Add(typeof(T), factoryMethod);
        }

        public T Create<T>()
            where T : class
        {
            if(TypeToFactories.TryGetValue(typeof(T), out var func))
            {
                return func() as T;
            }

            // tries to use paramaterless constructor
            return Activator.CreateInstance<T>();
        }
    }
}
