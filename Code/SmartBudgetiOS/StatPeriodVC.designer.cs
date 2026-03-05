// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace SmartBudgetiOS
{
	[Register ("StatPeriodVC")]
	partial class StatPeriodVC
	{
		[Outlet]
		UIKit.UIPickerView periodPicker { get; set; }

		[Outlet]
		UIKit.UIDatePicker picker { get; set; }

		[Outlet]
		UIKit.UIView tablePlace { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (periodPicker != null) {
				periodPicker.Dispose ();
				periodPicker = null;
			}

			if (picker != null) {
				picker.Dispose ();
				picker = null;
			}

			if (tablePlace != null) {
				tablePlace.Dispose ();
				tablePlace = null;
			}
		}
	}
}
