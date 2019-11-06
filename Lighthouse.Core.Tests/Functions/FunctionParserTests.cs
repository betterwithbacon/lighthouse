using System;
using System.Threading.Tasks;
using FluentAssertions;
using Lighthouse.Core.Functions;
using Xunit;

namespace Lighthouse.Core.Tests.Functions
{
    public class FunctionParserTests
    {
        [Fact]
        public void TryParse_ValidFunctionTest_ReturnsFunction()
        {
            var parser = new FunctionParser();

            parser.TryParse<string>("test", out var function).Should().BeFalse();
            function.Should().NotBeNull();
        }

        [Fact]
        public void TryParse_EmptyString_NoFunction()
        {
            var parser = new FunctionParser();

            parser.TryParse<string>("", out var function).Should().BeFalse();
            function.Should().BeNull();
        }

        [Fact]
        public async Task TryParse_SingleValueFunction_ReturnsCorrectValue()
        {
            var parser = new FunctionParser();

            parser.TryParse<int>("2 + 2", out var function).Should().BeFalse();
            var val = await function.Execute();

            val.Should().Be(4);
        }

        [Fact]
        public async Task TryParse_SingleArgument_ReturnsCorrectValue()
        {
            var input = new ClosuredObject
            {
                HasBeenMutated = false
            };

            var parser = new FunctionParser();

            parser.TryParse<ClosuredObject, bool>("HasBeenMutated = true", out var function).Should().BeFalse();
            _ = await function.Execute(input);
            input.HasBeenMutated.Should().BeTrue();
        }

        public class ClosuredObject
        {
            public bool HasBeenMutated { get; set; } = false;
            public object Value { get; set; }
        }
    }
}
