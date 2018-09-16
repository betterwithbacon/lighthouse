using System;

namespace Lighthouse.Core.Events
{
	public sealed class PointInTime
	{
		private PointInTime() { }
		
		public PointInTime(DateTime time)
		{

		}

		public DateTime Time
		{
			get;
			private set;
		}

		public static bool operator >(PointInTime pointInTime, DateTime date)
		{
			return pointInTime.Time > date;
		}

		public static bool operator <(PointInTime pointInTime, DateTime date)
		{
			return pointInTime.Time < date;
		}

		public static bool operator <=(PointInTime pointInTime, DateTime date)
		{
			return pointInTime.Time <= date;
		}

		public static bool operator >=(PointInTime pointInTime, DateTime date)
		{
			return pointInTime.Time >= date;
		}

		public static bool operator ==(PointInTime pointInTime, DateTime date)
		{
			return pointInTime.Time == date;
		}

		public static bool operator !=(PointInTime pointInTime, DateTime date)
		{
			return pointInTime.Time != date;
		}

		public override int GetHashCode()
		{
			return Time.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if(obj is PointInTime)			
				return ((PointInTime)obj).Time  == this.Time;
			
			return false;
		}
	}
}