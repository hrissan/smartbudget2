// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace SmartBudgetiOS
{
	[Register ("CurrencyCell")]
	partial class CurrencyCell
	{
		[Outlet]
		UIKit.UILabel labelSymbol { get; set; }

		[Outlet]
		UIKit.UILabel labelName { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (labelSymbol != null) {
				labelSymbol.Dispose ();
				labelSymbol = null;
			}

			if (labelName != null) {
				labelName.Dispose ();
				labelName = null;
			}
		}
	}
}
