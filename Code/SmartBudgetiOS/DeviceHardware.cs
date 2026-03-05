using System;
using System.Runtime.InteropServices;
using Foundation;
using UIKit;

namespace SmartBudgetiOS
{
	public class DeviceHardware
	{
/*		public const string HardwareProperty = "hw.machine";

		[DllImport(Constants.SystemLibrary)]
		static internal extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);

		public const string Unknown = "Unknown";
		public static string Version
		{
			get
			{
				var pLen = Marshal.AllocHGlobal(sizeof(int));
				sysctlbyname(DeviceHardware.HardwareProperty, IntPtr.Zero, pLen, IntPtr.Zero, 0);

				var length = Marshal.ReadInt32(pLen);

				if (length == 0)
				{
					Marshal.FreeHGlobal(pLen);

					return Unknown;
				}

				var pStr = Marshal.AllocHGlobal(length);
				sysctlbyname(DeviceHardware.HardwareProperty, pStr, pLen, IntPtr.Zero, 0);

				var hardwareStr = Marshal.PtrToStringAnsi(pStr);
				Marshal.FreeHGlobal(pLen);
				Marshal.FreeHGlobal(pStr);

				return hardwareStr;
			}
		}*/
	}
}