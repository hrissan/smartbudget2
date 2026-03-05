using System;
using Foundation;
using UIKit;
using CoreGraphics;
using System.Drawing;

namespace SmartBudgetiOS
{
	[Register ("UILabelSelectFont")]
	public class UILabelSelectFont : UILabel, UIFontSelect.Delegate
	{
		private nfloat original_size;
		public UILabelSelectFont (nfloat original_size, CGRect frame) : base (frame)
		{
			this.original_size = original_size;
			UIFontSelect.AddDelegate(this);
		}
		public UILabelSelectFont (IntPtr handle) : base (handle)
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

