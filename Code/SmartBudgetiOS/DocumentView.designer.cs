// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace SmartBudgetiOS
{
	[Register ("DocumentView")]
	partial class DocumentView
	{
		[Outlet]
		UIKit.UIButton btnSelect { get; set; }

		[Outlet]
		UIKit.UIImageView imgNew { get; set; }

		[Outlet]
		SmartBudgetiOS.UILabelSelectFont labelNew { get; set; }

		[Outlet]
		UIKit.UILabel labelSheets { get; set; }

		[Outlet]
		UIKit.UIView panelNew { get; set; }

		[Outlet]
		UIKit.UITableView table { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (btnSelect != null) {
				btnSelect.Dispose ();
				btnSelect = null;
			}

			if (imgNew != null) {
				imgNew.Dispose ();
				imgNew = null;
			}

			if (labelNew != null) {
				labelNew.Dispose ();
				labelNew = null;
			}

			if (labelSheets != null) {
				labelSheets.Dispose ();
				labelSheets = null;
			}

			if (table != null) {
				table.Dispose ();
				table = null;
			}

			if (panelNew != null) {
				panelNew.Dispose ();
				panelNew = null;
			}
		}
	}
}
