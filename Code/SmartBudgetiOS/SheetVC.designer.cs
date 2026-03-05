// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace SmartBudgetiOS
{
	[Register ("SheetVC")]
	partial class SheetVC
	{
		[Outlet]
		UIKit.UIButton btnHeaderPlan { get; set; }

		[Outlet]
		UIKit.UIImageView imgPlan { get; set; }

		[Outlet]
		SmartBudgetiOS.UILabelSelectFont labelPlan { get; set; }

		[Outlet]
		UIKit.UIView panelCloud { get; set; }

		[Outlet]
		UIKit.UIScrollView scrollDocs { get; set; }

		[Outlet]
		UIKit.UISearchBar searchBar { get; set; }

		[Outlet]
		UIKit.UIView tableHeader { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnHeaderPlan != null) {
				btnHeaderPlan.Dispose ();
				btnHeaderPlan = null;
			}

			if (panelCloud != null) {
				panelCloud.Dispose ();
				panelCloud = null;
			}

			if (scrollDocs != null) {
				scrollDocs.Dispose ();
				scrollDocs = null;
			}

			if (searchBar != null) {
				searchBar.Dispose ();
				searchBar = null;
			}

			if (tableHeader != null) {
				tableHeader.Dispose ();
				tableHeader = null;
			}

			if (labelPlan != null) {
				labelPlan.Dispose ();
				labelPlan = null;
			}

			if (imgPlan != null) {
				imgPlan.Dispose ();
				imgPlan = null;
			}
		}
	}
}
