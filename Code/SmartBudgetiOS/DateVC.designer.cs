// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace SmartBudgetiOS
{
	[Register ("DateVC")]
	partial class DateVC
	{
		[Outlet]
		UIKit.UILabel labelValue { get; set; }

		[Outlet]
		UIKit.UIDatePicker picker { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (picker != null) {
				picker.Dispose ();
				picker = null;
			}

			if (labelValue != null) {
				labelValue.Dispose ();
				labelValue = null;
			}

		}
	}
}
