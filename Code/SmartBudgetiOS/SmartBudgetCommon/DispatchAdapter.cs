using System;
#if SILVERLIGHT
#elif __ANDROID__
#else
using Foundation;
#endif

namespace SmartBudgetCommon
{
	public interface IDispatchOnUIThread {
		void Invoke (Action action);
	}
	#if SILVERLIGHT
	public class DispatchAdapter : IDispatchOnUIThread {
		public void Invoke (Action action) {
			Deployment.Current.Dispatcher.BeginInvoke(action);
		}
	}
	#elif __ANDROID__
	public class DispatchAdapter : IDispatchOnUIThread
	{
		public readonly Activity owner;
		public DispatchAdapter (Activity owner) {
			this.owner = owner;
		}
		public void Invoke (Action action) {
			owner.RunOnUiThread(action);
		}
	}
	#else
	public class DispatchAdapter : IDispatchOnUIThread
	{
		public readonly NSObject owner;
		public DispatchAdapter (NSObject owner) {
			this.owner = owner;
		}
		public void Invoke (Action action) {
			owner.BeginInvokeOnMainThread(action);
		}
	}
	#endif
}

