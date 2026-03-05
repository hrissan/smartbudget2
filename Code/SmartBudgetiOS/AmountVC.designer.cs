// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SmartBudgetiOS
{
	[Register ("AmountVC")]
	partial class AmountVC
	{
		[Outlet]
		UIKit.UIImageView imgSeparator { get; set; }

		[Outlet]
		UIKit.UILabel labelAccountName { get; set; }

		[Outlet]
		UIKit.UILabel labelAccountValue { get; set; }

		[Outlet]
		UIKit.UILabel labelAmount { get; set; }

		[Outlet]
		UIKit.UILabel labelCalc { get; set; }

		[Outlet]
		UIKit.UILabel labelConversionHint { get; set; }

		[Outlet]
		UIKit.UIView viewConversionHint { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (imgSeparator != null) {
				imgSeparator.Dispose ();
				imgSeparator = null;
			}

			if (labelAccountName != null) {
				labelAccountName.Dispose ();
				labelAccountName = null;
			}

			if (labelAccountValue != null) {
				labelAccountValue.Dispose ();
				labelAccountValue = null;
			}

			if (labelAmount != null) {
				labelAmount.Dispose ();
				labelAmount = null;
			}

			if (labelConversionHint != null) {
				labelConversionHint.Dispose ();
				labelConversionHint = null;
			}

			if (viewConversionHint != null) {
				viewConversionHint.Dispose ();
				viewConversionHint = null;
			}

			if (labelCalc != null) {
				labelCalc.Dispose ();
				labelCalc = null;
			}
		}
	}
}
