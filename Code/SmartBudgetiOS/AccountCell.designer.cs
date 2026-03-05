// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace SmartBudgetiOS
{
	[Register ("AccountCell")]
	partial class AccountCell
	{
		[Outlet]
		UIKit.UILabel labelName { get; set; }

		[Outlet]
		UIKit.UILabel labelValue { get; set; }

		[Outlet]
		UIKit.UIView viewLineParagon { get; set; }
		
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

			if (viewLineParagon != null) {
				viewLineParagon.Dispose ();
				viewLineParagon = null;
			}
		}
	}
}
