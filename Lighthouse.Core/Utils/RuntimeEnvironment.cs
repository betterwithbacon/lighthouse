﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Lighthouse.Server.Utils
{
    public static class RuntimeServices
    {
		public static OSPlatform GetOS()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return OSPlatform.Windows;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				return OSPlatform.Linux;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				return OSPlatform.OSX;

			return OSPlatform.Create("UNKNOWN");
		}
	}
}
