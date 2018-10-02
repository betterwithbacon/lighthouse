using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Storage
{
	/// <summary>
	/// A descriptor to describe how data might be stored. A given quantum of data might have multiple policies affecting it.
	/// </summary>
    public class StoragePolicy :IEqualityComparer<StoragePolicy>
    {
		public static readonly StoragePolicy Persistent;
		public static readonly StoragePolicy Archival;
		public static readonly StoragePolicy ColdStorage;
		public static readonly StoragePolicy Ephemeral;

		/// <summary>
		/// indicates that this data, should not survive the session, and also conversely, indicates if this data is available for the lifetime of a service
		/// An example is that while in-memory data isn't persistent, so it's not available after the session. Archival data might not be retrievable during he lifetime of the session
		/// </summary>
		public bool SessionScope { get; private set; }

		/// <summary>
		/// The amount of time, in DAYS, that the data should be available. Most data will be available for longer peri
		/// The day, is the lowest resolution of time available right now.
		/// 
		/// </summary>
		public int TimeToLive { get; private set; }

		/// <summary>
		/// The amount of time,in seconds, it should take to retrieve data. This is useful for cloud/archival sources. 
		/// After data is stored, the policies can/will change multiple times, so while data might start as ephemeral, the policies might change progressively over the course of one or multiple sessions.		
		/// </summary>
		public int RetrievalTimeout { get; private set; }

		public bool IsEquivalentTo(StoragePolicy requestedPolicy)
		{
			return
				requestedPolicy.Archive == this.Archive &&
				requestedPolicy.SessionScope == this.SessionScope &&
				Math.Abs(requestedPolicy.TimeToLive - this.TimeToLive) == 0 && // if the policy TTL is less than or equal to this policy. So that this policy is a superset
				Math.Abs(requestedPolicy.RetrievalTimeout - this.RetrievalTimeout) == 0;
		}

		/// <summary>
		/// Indicates that this data should be archived.
		/// There's an, intentional, overlap in how these policies might be intrepeted. It might be easier to define an archival policy, than the specific amount of time it will be available.
		/// Some storage doesn't support hourly/daily storage policies, so archival is a broad way to describe "longer term" durations.
		/// </summary>
		public bool Archive { get; private set; }

		/// <summary>
		/// Indefinite storage.
		/// </summary>
		public const int Infinity = Int32.MaxValue;

		public string Name { get; private set; }

		public StoragePolicy(string name)
		{
			Name = name;
		}

		static StoragePolicy()
		{
			Ephemeral = new StoragePolicy("Ephemeral")
			{
				SessionScope = true,
				TimeToLive = 0,
				Archive = false,
				RetrievalTimeout = 0 // data shuld be available right away
			};

			Persistent = new StoragePolicy("Persistent")
			{
				SessionScope = true,
				TimeToLive = Infinity,
				Archive = false,
				RetrievalTimeout = 0 // data shuld be available right away
			};

			Archival = new StoragePolicy("Archival")
			{
				SessionScope = true,
				TimeToLive = Infinity,
				Archive = true,
				RetrievalTimeout = 0 // 
			};

			ColdStorage = new StoragePolicy("ColdStorage")
			{
				SessionScope = true,
				TimeToLive = Infinity,
				Archive = true,
				RetrievalTimeout = 24 // 1 day to thaw it out? 
			};
		}

		public override string ToString()
		{
			return Name;
		}

		public bool Equals(StoragePolicy x, StoragePolicy y)
		{
			return x.GetHashCode() == y.GetHashCode();
		}

		public int GetHashCode(StoragePolicy obj)
		{
			return HashCode.Combine(
				this.Archive, this.SessionScope, this.TimeToLive, this.RetrievalTimeout);
		}
	}
}
