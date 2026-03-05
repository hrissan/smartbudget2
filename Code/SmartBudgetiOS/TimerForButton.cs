using System;
using UIKit;
using Foundation;

namespace SmartBudgetiOS
{
	public class TimerForButton
	{
		private Action click_action;
		private Action hold_action;
		public TimerForButton (UIButton btn, Action click_action, Action hold_action)
		{
			this.click_action = click_action;
			this.hold_action = hold_action;
			btn.TouchUpInside += (sender, e) => {
				stop_timer();
				this.click_action.Invoke();
			};
			btn.TouchCancel += (sender, e) => {
				stop_timer();
			};
			btn.TouchDown += (sender, e) => {
				reset_timer (delegate(NSTimer t) {
					btn.CancelTracking(null);
					this.hold_action.Invoke();
				});
			};
		}
		~TimerForButton()
		{
			Console.WriteLine("~TimerForButton");
		}
		private NSTimer hold_timer;
		private void stop_timer()
		{
			if (hold_timer != null) {
				hold_timer.Invalidate ();
				hold_timer.Dispose ();
			}
			hold_timer = null;
		}
		private void reset_timer(Action<NSTimer> hold_action)
		{
			stop_timer();
			hold_timer = NSTimer.CreateScheduledTimer (0.5, hold_action);
		}
	}
}

