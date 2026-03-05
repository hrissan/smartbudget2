// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace SmartBudgetiOS
{
	[Register ("PinVC")]
	partial class PinVC
	{
		[Outlet]
		UIKit.UITextField editPin1 { get; set; }

		[Outlet]
		UIKit.UITextField editPin2 { get; set; }

		[Outlet]
		UIKit.UIImageView imgSeparator { get; set; }

		[Outlet]
		SmartBudgetiOS.UILabelSelectFont labelPin1 { get; set; }

		[Outlet]
		SmartBudgetiOS.UILabelSelectFont labelPin2 { get; set; }

		[Outlet]
		UIKit.UIView slidingView { get; set; }

		[Outlet]
		UIKit.UIView tableParagon { get; set; }

		[Outlet]
		UIKit.UIView tablePlace { get; set; }

		[Action ("edit1Changed:")]
		partial void edit1Changed (UIKit.UITextField sender);

		[Action ("edit2Changed:")]
		partial void edit2Changed (UIKit.UITextField sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (editPin1 != null) {
				editPin1.Dispose ();
				editPin1 = null;
			}

			if (editPin2 != null) {
				editPin2.Dispose ();
				editPin2 = null;
			}

			if (labelPin1 != null) {
				labelPin1.Dispose ();
				labelPin1 = null;
			}

			if (labelPin2 != null) {
				labelPin2.Dispose ();
				labelPin2 = null;
			}

			if (slidingView != null) {
				slidingView.Dispose ();
				slidingView = null;
			}

			if (tableParagon != null) {
				tableParagon.Dispose ();
				tableParagon = null;
			}

			if (tablePlace != null) {
				tablePlace.Dispose ();
				tablePlace = null;
			}

			if (imgSeparator != null) {
				imgSeparator.Dispose ();
				imgSeparator = null;
			}
		}
	}
}
