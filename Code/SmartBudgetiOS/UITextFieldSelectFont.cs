using System;
using Foundation;
using UIKit;
using System.Drawing;

namespace SmartBudgetiOS
{
	[Register ("UITextFieldSelectFont")]
	public class UITextFieldSelectFont : UITextField, UIFontSelect.Delegate
	{
		private nfloat original_size;

		public UITextFieldSelectFont (IntPtr handle) : base (handle)
		{
		}
		public void on_font_changed(string font_name, float scale)
		{
			UIFont new_font = UIFont.FromName (font_name, (nfloat)Math.Floor(original_size * scale));
			Font = new_font;
		}
		public override void AwakeFromNib ()
		{
			original_size = Font.PointSize;
			UIFontSelect.AddDelegate(this);
		}
	}
}
