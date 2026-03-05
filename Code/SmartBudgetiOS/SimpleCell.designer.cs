// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace SmartBudgetiOS
{
	[Register ("SimpleCell")]
	partial class SimpleCell
	{
		[Outlet]
		UIKit.UILabel labelName { get; set; }

		[Outlet]
		UIKit.UIImageView imgImage { get; set; }

		[Outlet]
		UIKit.UILabel labelValue { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (labelName != null) {
				labelName.Dispose ();
				labelName = null;
			}

			if (imgImage != null) {
				imgImage.Dispose ();
				imgImage = null;
			}

			if (labelValue != null) {
				labelValue.Dispose ();
				labelValue = null;
			}
		}
	}
}
