using FluentAssertions;
using Lighthouse.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Lighthouse.Core.Tests.Utils
{
	public class ReflectionUtilTests
	{
		class TestClass
		{
			public void Method1(string arg1)
			{

			}
		}

		class TestClassWithSubclass
		{
			public void Method1(string arg1)
			{

			}
		}

		class SubClassOfTheTestClassWithSubclass : TestClassWithSubclass
		{
			public void Method1(int arg2)
			{

			}
		}

		public Type GivenAClass(Type type)
		{
			return type;
		}

		#region GetMethodsBySingleParameterType
		[Fact]
		public void GetMethodsBySingleParameterType_OneClass_Returns3Methods()
		{
			GivenAClass(typeof(TestClass))
				.WhenGetMethodsBySingleParameterTypeIsCalled(forMethod: "Method1")
				.ThenSingleMethodShouldExist(withName: "Method1")
				.AndOnlyOneParameterExists(withName: "arg1", andTypeOf: typeof(string));
		}

		[Fact]
		public void GetMethodsBySingleParameterType_SubClass_Returns1BaseMethod_And1SubclassMethod()
		{
			GivenAClass(typeof(SubClassOfTheTestClassWithSubclass))
				.WhenGetMethodsBySingleParameterTypeIsCalled(forMethod: "Method1")
				.ThenMultipleMethodsShouldExist(typeof(string), typeof(int));
		}
		#endregion

		[Fact]
		public void GetMethodsBySingleParameterType_Tier3SubClass_Returns1BaseMethod_And1SubclassMethod_And1LeafMethod()
		{
            // TODO: implement			
		}
	}

	static class ReflectionUtilTestExtensions
	{
		public static IDictionary<Type, MethodInfo> WhenGetMethodsBySingleParameterTypeIsCalled(this Type type, string forMethod)
		{
			return ReflectionUtil.GetMethodsBySingleParameterType(type, forMethod);
		}

		public static MethodInfo ThenSingleMethodShouldExist(this IDictionary<Type, MethodInfo> input, string withName)
		{
			input.Count.Should().Be(1);
			var method = input.Single().Value;
			method.Name.Should().Be(withName);  
			return input.Single().Value;
		}

		public static ParameterInfo AndOnlyOneParameterExists(this MethodInfo method, string withName, Type andTypeOf)
		{
			var methodSignatureParam = method.GetParameters().Single();
			methodSignatureParam.Name.Should().Be(withName);
			methodSignatureParam.ParameterType.Should().Be(andTypeOf);
			return methodSignatureParam;
		}

		public static IDictionary<Type, MethodInfo> ThenMultipleMethodsShouldExist(this IDictionary<Type, MethodInfo> input,
			params Type[] exepectedTypes)
		{
			input.Count.Should().BeGreaterThan(1);
			// it should be a superset
			input.Values.Select(mi => mi.GetParameters().Single().ParameterType).Except(exepectedTypes).Should().BeEmpty();
			return input;
		}
	}
}
