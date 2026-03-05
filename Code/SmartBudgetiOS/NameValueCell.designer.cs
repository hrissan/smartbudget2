// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace SmartBudgetiOS
{
	[Register ("NameValueCell")]
	partial class NameValueCell
	{
		[Outlet]
		UIKit.UIImageView icon { get; set; }

		[Outlet]
		UIKit.UILabel comment { get; set; }

		[Outlet]
		UIKit.UILabel name { get; set; }

		[Outlet]
		UIKit.UILabel value { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (icon != null) {
				icon.Dispose ();
				icon = null;
			}

			if (comment != null) {
				comment.Dispose ();
				comment = null;
			}

			if (name != null) {
				name.Dispose ();
				name = null;
			}

			if (value != null) {
				value.Dispose ();
				value = null;
			}
		}
	}
}
