using System;
using Foundation;

namespace FlurryAnalytics
{
	public static class Flurry {
		public static void SetAppVersion (string version)
		{}

		//		public static string GetFlurryAgentVersion ()
//		{}

		public static void SetShowErrorInLog (bool value)
		{}

		public static void SetDebugLog (bool value)
		{}

		public static void SetSessionContinue (int seconds)
		{}

		public static void SetSecureTransportEnabled (bool value)
		{}

		public static void StartSession (string apiKey)
		{}

		public static void LogEvent (string eventName)
		{}

		public static void LogEvent (string eventName, NSDictionary parameters)
		{}

		public static void LogError (string errorID, string message, NSException exception)
		{}

		public static void LogError (string errorID, string message, NSError error)
		{}

		public static void LogEvent (string eventName, bool timed)
		{}

		public static void LogEvent (string eventName, NSDictionary parameters, bool timed)
		{}

		public static void EndTimedEvent (string eventName, NSDictionary parameters)
		{}

		public static void LogAllPageViews (NSObject target)
		{}

		public static void LogPageView ()
		{}

		public static void SetUserID (string userID)
		{}

		public static void SetAge (int age)
		{}

		public static void SetGender (string gender)
		{}

		public static void SetSessionReportsOnClose (bool sendSessionReportsOnClose)
		{}

		public static void SetSessionReportsOnPause (bool setSessionReportsOnPauseEnabled)
		{}

		public static void SetEventLogging (bool value)
		{}
	}
}
