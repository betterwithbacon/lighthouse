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
}
