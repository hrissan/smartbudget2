// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace SmartBudgetiOS
{
	[Register ("SettingsVC")]
	partial class SettingsVC
	{
		[Outlet]
		UIKit.UITableView table { get; set; }

		[Outlet]
		UIKit.UIButton btnRestore { get; set; }

		[Outlet]
		UIKit.UIButton btnPurchase { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView activityPurchasing { get; set; }

		[Outlet]
		UIKit.UILabel labelStatus { get; set; }

		[Outlet]
		UIKit.UILabel labelLimit { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (table != null) {
				table.Dispose ();
				table = null;
			}

			if (btnRestore != null) {
				btnRestore.Dispose ();
				btnRestore = null;
			}

			if (btnPurchase != null) {
				btnPurchase.Dispose ();
				btnPurchase = null;
			}

			if (activityPurchasing != null) {
				activityPurchasing.Dispose ();
				activityPurchasing = null;
			}

			if (labelStatus != null) {
				labelStatus.Dispose ();
				labelStatus = null;
			}

			if (labelLimit != null) {
				labelLimit.Dispose ();
				labelLimit = null;
			}
		}
	}
}
