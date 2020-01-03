using System;
using FluentAssertions;
using Lighthouse.Core.Utils;
using Xunit;

namespace Lighthouse.Core.Tests.Utils
{
    public class TypeFactoryTests
    {
        [Fact]
        public void Create_WithInterface()
        {
            var typeFactory = new TypeFactory();

            // for this interface type, creat ea concrete
            typeFactory.Register<Interface>(() => new Concrete());

            var created = typeFactory.Create<Interface>();
            created.GetType().Should().Be(typeof(Concrete));
        }

        [Fact]
        public void Create_WithConcrete()
        {
            var typeFactory = new TypeFactory();

            // for this interface type, creat ea concrete
            typeFactory.Register<Interface>(() => new Concrete());

            var created = typeFactory.Create<Interface>();
            created.GetType().Should().Be(typeof(Concrete));
        }

        [Fact]
        public void Create_WithNoMapping()
        {
            var typeFactory = new TypeFactory();

            var created = typeFactory.Create<Concrete>();
            created.GetType().Should().Be(typeof(Concrete));
        }

        public interface Interface
        {

        }

        public class Concrete : Interface
        {

        }
    }
}
