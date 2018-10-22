﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core
{
	public sealed class LighthouseServerStatus
	{
		public Version Version { get; }
		public string ServerName { get; }
		public DateTime ServerTime { get; }

		public LighthouseServerStatus(Version version, string serverName, DateTime serverTime)
		{
			Version = version;
			ServerName = serverName;
			ServerTime = serverTime;
		}

		public static bool TryParse(string input, out LighthouseServerStatus status)
		{
			try
			{
				status = JsonConvert.DeserializeObject<LighthouseServerStatus>(input);
				return true;
			}
			catch(Exception e)
			{
				status = null;
				return false;
			}			
		}
	}
}