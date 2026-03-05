// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace SmartBudgetiOS
{
	[Register ("NameVC")]
	partial class NameVC
	{
		[Outlet]
		UIKit.UIButton btnContacts { get; set; }

		[Outlet]
		UIKit.UIImageView imgSeparator { get; set; }

		[Outlet]
		UIKit.UITextField name { get; set; }

		[Outlet]
		UIKit.UITextField nameZlo { get; set; }

		[Outlet]
		UIKit.UITableView table { get; set; }

		[Outlet]
		UIKit.UIView tableParagon { get; set; }

		[Outlet]
		UIKit.UIView tablePlace { get; set; }

		[Action ("nameChanged:")]
		partial void nameChanged (UIKit.UITextField sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (btnContacts != null) {
				btnContacts.Dispose ();
				btnContacts = null;
			}

			if (name != null) {
				name.Dispose ();
				name = null;
			}

			if (nameZlo != null) {
				nameZlo.Dispose ();
				nameZlo = null;
			}

			if (table != null) {
				table.Dispose ();
				table = null;
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
