// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace SmartBudgetiOS
{
	[Register ("ExpenseCell")]
	partial class ExpenseCell
	{
		[Outlet]
		UIKit.UIImageView imgCategory { get; set; }

		[Outlet]
		UIKit.UIImageView imgIndicator { get; set; }

		[Outlet]
		UIKit.UILabel labelName { get; set; }

		[Outlet]
		UIKit.UILabel labelValue { get; set; }

		[Outlet]
		UIKit.UIImageView viewProgress { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (labelName != null) {
				labelName.Dispose ();
				labelName = null;
			}

			if (labelValue != null) {
				labelValue.Dispose ();
				labelValue = null;
			}

			if (imgIndicator != null) {
				imgIndicator.Dispose ();
				imgIndicator = null;
			}

			if (imgCategory != null) {
				imgCategory.Dispose ();
				imgCategory = null;
			}

			if (viewProgress != null) {
				viewProgress.Dispose ();
				viewProgress = null;
			}
		}
	}
}
