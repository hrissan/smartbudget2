// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace SmartBudgetiOS
{
	[Register ("ExpenseVC")]
	partial class ExpenseVC
	{
		[Outlet]
		UIKit.UIImageView warningRed { get; set; }

		[Outlet]
		UIKit.UIImageView warningGreen { get; set; }

		[Outlet]
		UIKit.UILabel labelWarning { get; set; }

		[Outlet]
		UIKit.UIView viewWarning { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (warningRed != null) {
				warningRed.Dispose ();
				warningRed = null;
			}

			if (warningGreen != null) {
				warningGreen.Dispose ();
				warningGreen = null;
			}

			if (labelWarning != null) {
				labelWarning.Dispose ();
				labelWarning = null;
			}

			if (viewWarning != null) {
				viewWarning.Dispose ();
				viewWarning = null;
			}
		}
	}
}
