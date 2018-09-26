using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Lighthouse.Core.Tests.Scheduling
{
	public class ScheduleTests
	{
		[Fact]
		public void WhenGetNextRunTimeIsCalled_WithOnce_NRTIsThatDate()
		{
			true.Should().BeFalse();
		}

		[Fact]
		public void WhenGetNextRunTimeIsCalled_WithDaily_NRTIsThatDate()
		{
			true.Should().BeFalse();
		}

		[Fact]
		public void WhenGetNextRunTimeIsCalled_WithHourly_NRTIsThatDate()
		{
			true.Should().BeFalse();
		}

		[Fact]
		public void WhenGetNextRunTimeIsCalled_WithMinutely_NRTIsThatDate()
		{
			true.Should().BeFalse();
		}

		[Fact]
		public void WhenGetNextRunTimeIsCalled_WithSecondly_NRTIsThatDate()
		{
			true.Should().BeFalse();
		}
	}
}
