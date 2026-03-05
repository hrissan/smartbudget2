// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace SmartBudgetiOS
{
	[Register ("ImageSquareView")]
	partial class ImageSquareView
	{
		[Outlet]
		UIKit.UIButton btnSelect { get; set; }

		[Outlet]
		UIKit.UIImageView imgSelected { get; set; }

		[Outlet]
		UIKit.UIImageView imgCategory { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (btnSelect != null) {
				btnSelect.Dispose ();
				btnSelect = null;
			}

			if (imgSelected != null) {
				imgSelected.Dispose ();
				imgSelected = null;
			}

			if (imgCategory != null) {
				imgCategory.Dispose ();
				imgCategory = null;
			}
		}
	}
}
