// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace SmartBudgetiOS
{
	[Register ("DrawVC")]
	partial class DrawVC
	{
		[Outlet]
		UIKit.UIView drawPlace { get; set; }

		[Outlet]
		UIKit.UIImageView imgResult { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (imgResult != null) {
				imgResult.Dispose ();
				imgResult = null;
			}

			if (drawPlace != null) {
				drawPlace.Dispose ();
				drawPlace = null;
			}
		}
	}
}
